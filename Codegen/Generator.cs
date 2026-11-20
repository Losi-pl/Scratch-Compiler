#pragma warning disable CS8618
using System.Collections.Frozen;
using Humanizer;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Entries;
using Target = ScratchCompiler.Parser.Target;

namespace ScratchCompiler.Codegen;

public static partial class Generator
{
    [ThreadStatic] internal static CodegenTools Current;
    
    public static CodeSet GenerateCode(this SyntaxTree tree, string? projectName = null)
    {
        var context = LLVMContextRef.Create();
        var module = context.CreateModuleWithName(projectName ?? "scratch-app");
        var builder = context.CreateBuilder();

        Current = new()
        {
            Context = context,
            Module = module,
            Builder = builder,
            ResourceModule = context.CreateModuleWithName($"{module.SourceFileName}.resources")
        };
        Current.Libs = new(Current);
        MapActorNames(tree);
        
        var main = LLVMMethod.CreateMethod(context.VoidType, "main", []);
        var actorSetup = LLVMMethod.CreateMethod(context.VoidType, "actor_setup", []);
        var actorEnumeration = GenActorEnumerationBody(builder);
        var scriptUpdates = GenRunScriptUpdates();
        var runOnStartScripts = GenRunOnStartScripts();

        DeclareScriptMethods(tree);
        GenerateOnStartMethods();
        GenerateActorsDeclaration(builder, tree, actorSetup);
        
        // Build the body of `main`
        {
            
            var mainBlock = main.Function.AppendBasicBlock("main");
            builder.PositionAtEnd(mainBlock);
            
            builder.BuildCall2(Current.Libs.ScratchInit, []);
            builder.BuildCall2(Current.Libs.InitWindow, [builder.BuildGlobalString(module.SourceFileName, "Scratch::Project-Name")]);
            builder.BuildCall2(actorSetup, []);
            builder.BuildCall2(actorEnumeration, [runOnStartScripts.Function, Current.Libs.Nullptr]);
            builder.BuildCall2(actorEnumeration, [scriptUpdates.Function, Current.Libs.Nullptr]);
            builder.BuildCall2(Current.Libs.RunApp, []);

            builder.BuildCall2(Current.Libs.ExitApp, [LLVMValueRef.CreateConst(0)]);
            builder.BuildUnreachable();
        }
        
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        return new(context, [Current.Module], [Current.ResourceModule]);
    }

    private static void MapActorNames(SyntaxTree tree)
    {
        Dictionary<Target, CharPointer> local = new();
        foreach (var target in tree.GetAllTargets())
            local[target] = Current.Builder.CreateGlobalConstString(target.Name, target.Name + "::Name");
        Current.TargetNames = local.ToFrozenDictionary();
    }

    private static void GenerateActorsDeclaration(LLVMBuilderRef builder, SyntaxTree tree, LLVMMethod actorSetup)
    {
        if (tree.Actors.Count == 0)
        {
            builder.PositionAtEnd(actorSetup.Function.AppendBasicBlock("empty"));
            builder.BuildRetVoid();
            return;
        }
        
        LLVMBasicBlockRef? entryBlock = null;
        var actorSize = LLVMValueRef.CreateConst(Current.Libs.ActorStruct.ByteSize);
        foreach (var actor in tree.Actors.Reverse())
        {
            if (entryBlock.HasValue) {
                entryBlock = actorSetup.Function.AppendBasicBlock("Construct: " + actor.Name);
                builder.BuildBr(entryBlock.Value);
                builder.PositionAtEnd(entryBlock.Value);
            }
            else {
                entryBlock = actorSetup.Function.AppendBasicBlock("Construct: " + actor.Name);
                builder.PositionAtEnd(entryBlock.Value);
            }

            var scripts = Current.Scripts[actor];
            var posX = LLVMValueRef.CreateConst(actor.DefaultPosition.x);                                             // Const: Start position x
            var posY = LLVMValueRef.CreateConst(actor.DefaultPosition.y);                                             // Const: Start position y
            var scriptCount = LLVMValueRef.CreateConst(scripts.Scripts.Length);                                       // Const: Count of Actor scripts
            var defCostume = LLVMValueRef.CreateConst(actor.DefaultCostume);                                          // Const: Default Costume index
            var costumeCount = LLVMValueRef.CreateConst((uint)actor.Costumes.Length);                                 // Const: Costume count
            var actorScale = LLVMValueRef.CreateConst(actor.DefaultSize);                                             // Const: Actor size
            
            var inst = builder.BuildCall2(Current.Libs.Malloc, [actorSize], actor.Name);                  // Allocate space for an Actor
            builder.BuildCall2(Current.Libs.RegisterActor, [inst]);                                                   // Register the actor
            var costumes = builder.BuildCall2(Current.Libs.CreateCostumeStorage, [costumeCount],          // Create costume storage
                $"{actor.Name}::Costumes");                                                                      // ^^^
            builder.BuildCall2(Current.Libs.ConstructActor, [inst, Current.TargetNames[actor],                        // Construct actor's body
                Current.Scripts[actor].Events.Struct, posX, posY, scriptCount, costumes, defCostume, actorScale]);    // ^^^
            var scriptField = builder.BuildStructGEP2(Current.Libs.ActorStruct, inst, 4,           // Find field of script array
                $"field: {actor.Name}::Scripts");                                                               // ^^^
            var scriptArray = builder.BuildLoad2(Current.Libs.ScriptHolder.Pointer, scriptField,          // Get script list
                    $"{actor.Name}::Scripts");                                                                  // ^^^

            
            foreach (var costume in actor.Costumes.Zip(Current.Scripts[actor].costumes).Select((c, i) => (sys: c.First, llvm: c.Second, ind: i)))
            {
                var index = LLVMValueRef.CreateConst((uint)costume.ind);
                var name = builder.BuildGlobalString(costume.sys.Name, $"{actor.Name}::{costume.sys.AssetID}::Name");
                if (costume.sys.FileFormat == CostumeFormat.SVG)
                    builder.BuildCall2(Current.Libs.LoadCostumeFromSvg, [costumes, index, costume.llvm.Content, name]);
                else
                {
                    var size = builder.BuildLoad2(Current.Context.Int64Type, costume.llvm.Size!.Value, $"{actor.Name}::{costume.sys.AssetID}::Size");   
                    builder.BuildCall2(Current.Libs.LoadCostumeFromData, [costumes, index, costume.llvm.Content, size, name]);
                }
            }
            
            for (int i = 0; i < scripts.Scripts.Length; i++)
            {
                ref var script = ref scripts.Scripts[i];
                var scriptPtr = builder.BuildGEP2(Current.Libs.ScriptHolder, scriptArray,     // Scripts[i]
                    new[] { LLVMValueRef.CreateConst(i) }, $"{actor.Name}::Script[{i}]");       // ^^^
                builder.BuildCall2(Current.Libs.ConstructScript,                                             // Construct script handler
                    [scriptPtr, LLVMValueRef.CreateConst(script.StackSize)]);                                // ^^^
            }
        }

        builder.BuildRetVoid();
    }

    private static void GenerateOnStartMethods()
    {
        foreach (var target in Current.Scripts)
        {
            for (int i = 0; i < target.Value.Scripts.Length; i++)
            {
                ref var method = ref target.Value.Scripts[i];
                if(method.Script.Entry is not OnStart)
                    continue;
                
                method.StackSize = GenScript.GenerateScript(target.Key, method.Script, method.Function);
                method.IsDefined = true;
            }
        }

    }

    internal static Exception NotImplemented(object element) => (Exception)System.Reflection.Assembly.GetAssembly(typeof(System.Reflection.Assembly))!
        .GetType("System.NotImplementedException")!.GetConstructors()[1].Invoke([$"Element {element.ToString().Truncate(32)} is not yet implemented"]);
}