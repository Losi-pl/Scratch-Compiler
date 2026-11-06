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
    
    public static LLVMSharp.Module GenerateCode(this SyntaxTree tree, string? projectName = null)
    {
        var contextCs = new LLVMSharp.LLVMContext();
        var moduleCs = LLVMSharp.Module.Create(contextCs, projectName ?? "scratch-app");
        var builderCs = new LLVMSharp.IRBuilder(contextCs);

        var context = contextCs.Handle;
        var module = moduleCs.Handle;
        var builder = builderCs.Handle;

        Current = new()
        {
            Context = context,
            Module = module,
            Builder = builder
        };
        Current.Libs = new(Current);
        MapActorNames(tree);
        
        var main = LLVMMethod.CreateMethod(context.VoidType, "main", []);
        var actorSetup = LLVMMethod.CreateMethod(context.VoidType, "actor_setup", []);
        var actorEnumeration = GenActorEnumerationBody(builder);
        var scriptUpdates = GenRunScriptUpdates();

        DeclareScriptMethods(tree);
        GenerateOnStartMethods();
        GenerateActorsDeclaration(builder, tree, actorSetup);
        
        // Build the body of `main`
        {
            
            var mainBlock = main.Function.AppendBasicBlock("main");
            builder.PositionAtEnd(mainBlock);
            
            builder.BuildCall2(Current.Libs.ScratchInit, []);
            builder.BuildCall2(actorSetup, []);
            builder.BuildCall2(actorEnumeration, [scriptUpdates.Function, LLVMValueRef.CreateConstPointerNull(Current.Libs.PointerType)]);

            builder.BuildRetVoid();
        }
        
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        return moduleCs;
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
        foreach (var actor in tree.Actors)
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
            var scriptCount = LLVMValueRef.CreateConst(scripts.scripts.Length);                                       // Const: Count of actor scripts
            
            var inst = builder.BuildCall2(Current.Libs.Malloc, [actorSize], actor.Name);                  // Allocate space for an Actor
            builder.BuildCall2(Current.Libs.RegisterActor, [inst]);                                                   // Register the actor
            builder.BuildCall2(Current.Libs.ConstructActor,                                                           // Construct actor's body
                [inst, Current.TargetNames[actor], Current.Scripts[actor].engine.Function, posX, posY, scriptCount]); // ^^^
            var scriptField = builder.BuildStructGEP2(Current.Libs.ActorStruct, inst, 4);          // Find field of script array
            var scriptArray = builder.BuildLoad2(Current.Libs.ScriptHolder.Pointer, scriptField,          // Get script list
                    $"{actor.Name}::Scripts");                                                                  // ^^^

            for (int i = 0; i < scripts.scripts.Length; i++)
            {
                ref var script = ref scripts.scripts[i];
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
            for (int i = 0; i < target.Value.scripts.Length; i++)
            {
                ref var method = ref target.Value.scripts[i];
                if(method.Script.Entry is not OnStart)
                    continue;
                
                method.StackSize = GenScript.GenerateScript(target.Key, method.Script, method.Function);
                method.IsDefined = true;
            }
        }

    }

    internal static Exception NotImplemented(object element) => 
        new NotImplementedException($"Element {element.ToString().Truncate(32)} is not yet implemented");
    
    public static void Compile(this LLVMSharp.Module module, string path)
    {
        var triple = LLVMTargetRef.DefaultTriple;
        
        // Create a target machine and emit an object file
        var target = LLVMTargetRef.GetTargetFromTriple(triple);
        var targetMachine = target.CreateTargetMachine(
            triple, "generic", "",
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault
        );

        var objectFile = Path.GetTempFileName();
        
        targetMachine.EmitToFile(module.Handle, objectFile, LLVMCodeGenFileType.LLVMObjectFile);

        #if DEBUG
        var sysroot = Environment.GetEnvironmentVariable("llvm-sysroot");
        #else
        var sysroot = Path.Combine(AppContext.BaseDirectory, "libs/");
        #endif
        
        using var linkProcess = System.Diagnostics.Process.Start(
            // ReSharper disable UseCollectionExpression, StringLiteralTypo
            "lld-link", new[] { objectFile, 
                $"/out:{path}",
                Path.Combine(AppContext.BaseDirectory, "libScratch.lib"), 
                "/defaultlib:libcmt.lib", 
                "/defaultlib:libvcruntime.lib",
                $"/libpath:{sysroot}/crt/lib/x86_64",
                $"/libpath:{sysroot}/sdk/lib/um/x86_64",
                $"/libpath:{sysroot}/sdk/lib/ucrt/x86_64",
                "/noimplib"
            }
            // ReSharper restore UseCollectionExpression, StringLiteralTypo
        );
        linkProcess.WaitForExit();
        
        File.Delete(objectFile);
    }
}