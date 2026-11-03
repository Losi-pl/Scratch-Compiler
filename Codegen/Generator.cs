#pragma warning disable CS8618
using System.Collections.Frozen;
using Humanizer;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Entries;
using Target = ScratchCompiler.Parser.Target;

namespace ScratchCompiler.Codegen;

public static class Generator
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
        var actorEnumeration = LLVMMethod.CreateMethod(context.VoidType, "actor_enumeration", []);
        LocalActorEnumerationBody(builder, actorEnumeration);

        DeclareScriptMethods(tree);
        GenerateOnStartMethods(tree);
        GenerateActorsDeclaration(builder, tree, actorSetup);
        
        // Build the body of `main`
        {
            var mainBlock = main.Function.AppendBasicBlock("entry");
            builder.PositionAtEnd(mainBlock);
            
            builder.BuildCall2(Current.Libs.ScratchInit, []);
            builder.BuildCall2(actorSetup, []);
            //builder.BuildCall2(actorEnumeration, []);
            builder.BuildCall2(Current.Libs.RunScripts, []);

            builder.BuildRetVoid();
        }
        
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        return moduleCs;
    }

    private static void LocalActorEnumerationBody(LLVMBuilderRef builder, LLVMMethod fun)
    {
        var mainBlock = fun.Function.AppendBasicBlock("entry");
        builder.PositionAtEnd(mainBlock);
                                                                                                        // entry:
        var iter = builder.BuildAlloca(Current.Libs.ListHandler, "iterator");          // Stackalloc iteration spot
        var actors = builder.BuildCall2(Current.Libs.GetActors, [], "actors");             // Get the list

        builder.BuildCall2(Current.Libs.GetActorIterator, [iter]);                                          // Get actor iterator

        // ReSharper disable InconsistentNaming
        var loop_test = fun.Function.AppendBasicBlock("loop_test");
        var loop_body = fun.Function.AppendBasicBlock("loop_body");
        // ReSharper restore InconsistentNaming

        var exit = fun.Function.AppendBasicBlock("exit");

        builder.BuildBr(loop_test);                                                                         // entry -> loop_test
        builder.PositionAtEnd(loop_test);                                                               // loop_test:
        var actor = builder.BuildCall2(Current.Libs.IterateNext,                                // Get next actor
            [iter, actors, LLVMValueRef.CreateConst(false)], "actor");                                 // ^^^
        var nullptr = LLVMValueRef.CreateConstPointerNull(Current.Libs.ActorStruct.Pointer);                // Const: nullptr
        var condition = builder.BuildICmp(                                                      // is {actor == null}
            LLVMIntPredicate.LLVMIntNE, actor, nullptr, "condition");                   // ^^
        builder.BuildCondBr(condition, loop_body, exit);                                           // if condition goto loop_body else exit

        builder.PositionAtEnd(loop_body);                                                               // loop_body:
        var charPtr = LLVMTypeRef.CreatePointer(Current.Context.Int8Type, 0);                   // char*
        var actorNameField = builder.BuildStructGEP2(Current.Libs.ActorStruct, actor, 0);// Actor -> Name
        var actorName = builder.BuildLoad2(charPtr, actorNameField, "actor_name");        // Get actor name
        builder.BuildPrintLine(actorName);                                                               // Print(Actor->Name)

        builder.BuildBr(loop_test);                                                                         // Loop back

        builder.PositionAtEnd(exit);
        builder.BuildRetVoid();
    }

    private static void MapActorNames(SyntaxTree tree)
    {
        Dictionary<Target, CharPointer> local = new();
        foreach (var target in tree.GetAllTargets())
            local[target] = Current.Builder.CreateGlobalConstString(target.Name, target.Name + "::Name");
        Current.TargetNames = local.ToFrozenDictionary();
    }

    private static void DeclareScriptMethods(SyntaxTree tree)
    {
        var args = new[] {
                Current.Libs.PointerType,           // void* scripts_internal_args
                Current.Libs.ActorStruct.Pointer,   // Actor* actor
            };
        var funType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, args, false);
        Dictionary<Target, (CodeBlock script, LLVMMethod method, bool defined, uint stackSize)[]> scriptList = [];
        
        foreach (var target in tree.GetAllTargets())
        {
            var onStarts = 0u;
            var onClick = 0u;
            var onBackgroundChange = 0u;
            var onBroadcast = 0u;
            var onCloned = 0u;
            var onGraterThan = 0u;
            var onKeyPress = 0u;

            List<(CodeBlock script, LLVMMethod method, bool defined, uint stackSize)> scripts = [];
            
            foreach (var script in target.Blocks)
            {
                if(script.Entry is Dormant)
                    continue;

                var name = $"{target.Name}::{script.Entry switch {
                    OnStart=> $"OnStart_{onStarts++}",
                    OnKeyPress => $"OnKeyPress_{onKeyPress++}",
                    OnBroadcast => $"OnBroadcast_{onBroadcast++}",
                    OnActorClick => $"OnClick_{onClick++}",
                    OnStageClick => $"OnClick_{onClick++}",
                    OnBackgroundChangedTo => $"OnBackgroundChange_{onBackgroundChange++}",
                    OnCloneCreated => $"OnCloned_{onCloned++}",
                    OnGraterThan => $"OnGraterThan_{onGraterThan++}",
                    _ => throw NotImplemented(script.Entry)
                }}::Body";
                
                var fun = Current.Module.AddFunction(name, funType);
                scripts.Add((script, new(fun, funType, Current.Context.VoidType, args, false), false, 0));
                
                unsafe
                {
                    using var stackArg = new MarshaledString("scripts_stack");
                    LLVM.SetValueName(LLVM.GetParam(fun, 0), stackArg);
                    using var actorArg = new MarshaledString("actor");
                    LLVM.SetValueName(LLVM.GetParam(fun, 1), actorArg);
                }
            }

            scriptList[target] = scripts.ToArray();
        }

        Current.Scripts = scriptList.ToFrozenDictionary();
    }

    private static void GenerateActorsDeclaration(LLVMBuilderRef builder, SyntaxTree tree, LLVMMethod actorSetup)
    {
        LLVMBasicBlockRef? entryBlock = null;

        var actorSize = LLVMValueRef.CreateConst(Current.Libs.ActorStruct.ByteSize);
        foreach (var actor in tree.Actors)
        {
            if (entryBlock.HasValue)
            {
                entryBlock = actorSetup.Function.AppendBasicBlock("Init: " + actor.Name);
                builder.BuildBr(entryBlock.Value);
                builder.PositionAtEnd(entryBlock.Value);
            }
            else
            {
                entryBlock = actorSetup.Function.AppendBasicBlock("Init: " + actor.Name);
                builder.PositionAtEnd(entryBlock.Value);
            }

            var scripts = Current.Scripts[actor];

            var posX = LLVMValueRef.CreateConst(actor.DefaultPosition.x);                                   // Const: Start position x
            var posY = LLVMValueRef.CreateConst(actor.DefaultPosition.y);                                   // Const: Start position y
            var scriptCount = LLVMValueRef.CreateConst(scripts.Length);                                     // Const: Count of actor scripts
            var inst = builder.BuildCall2(Current.Libs.Malloc, [actorSize], actor.Name);        // Allocate space for an Actor
            builder.BuildCall2(Current.Libs.RegisterActor, [inst]);                                         // Register the actor
            builder.BuildCall2(Current.Libs.ConstructActor,                                                 // Construct actor's body
                [inst, Current.TargetNames[actor], posX, posY, scriptCount]);                               // ^^^

            var scriptField = builder.BuildStructGEP2(Current.Libs.ActorStruct, inst, 3);// Find field of script array
            var scriptArray = builder.BuildLoad2(Current.Libs.ScriptHolder.Pointer, scriptField,// Get script list
                    $"{actor.Name}::Scripts");                                                        // ^^^

            for (int i = 0; i < scripts.Length; i++)
            {
                ref var script = ref scripts[i];
                var scriptPtr = builder.BuildGEP2(Current.Libs.ScriptHolder, scriptArray,    // Scripts[i]
                    new[] { LLVMValueRef.CreateConst(i) }, $"{actor.Name}::Script[{i}]");      // ^^^
                builder.BuildCall2(Current.Libs.ConstructScript,                                            // Construct script handler
                    [scriptPtr, script.method.Function, LLVMValueRef.CreateConst(script.stackSize)]);       // ^^^
            }
        }

        builder.BuildRetVoid();
    }

    private static void GenerateOnStartMethods(SyntaxTree tree)
    {
        foreach (var target in Current.Scripts)
        {
            for (int i = 0; i < target.Value.Length; i++)
            {
                ref var method = ref target.Value[i];
                if(method.script.Entry is not OnStart)
                    continue;
                
                method.stackSize = GenMethodCalls.GenerateScript(target.Key, method.script, method.method);
                method.defined = true;
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