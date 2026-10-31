using System.Collections.Frozen;
using System.Collections.Immutable;
using Humanizer;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Entries;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Codegen;

public static class Generator
{
#pragma warning disable CS8618
    [ThreadStatic] internal static CodegenTools Current;
#pragma warning restore CS8618
    
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

        // ReSharper disable once UseCollectionExpression
        Current.OnStartMethods = GenerateOnStartMethods(tree).ToImmutableArray();
        
        {
            var entryBlock = actorSetup.Function.AppendBasicBlock("entry");
            builder.PositionAtEnd(entryBlock);
            
            var actorSize = LLVMValueRef.CreateConst(Current.Libs.ActorStruct.ByteSize);
            foreach (var actor in tree.Actors)
            {
                var posX = LLVMValueRef.CreateConst(actor.DefaultPosition.x);
                var posY = LLVMValueRef.CreateConst(actor.DefaultPosition.y);
                var inst = builder.BuildCall2(Current.Libs.Malloc, [actorSize], actor.Name);
                builder.BuildCall2(Current.Libs.ConstructActor, [inst, Current.TargetNames[actor], posX, posY]);
                builder.BuildCall2(Current.Libs.RegisterActor, [inst]);
            }

            builder.BuildRetVoid();
        }
        
        // Build the body of `main`
        {
            var mainBlock = main.Function.AppendBasicBlock("entry");
            builder.PositionAtEnd(mainBlock);
            builder.BuildCall2(Current.Libs.ScratchInit, []);

            builder.BuildCall2(actorSetup, []);
            builder.BuildCall2(actorEnumeration, []);

            foreach (var method in Current.OnStartMethods)
                builder.BuildCall2(method, []);
            builder.BuildRetVoid();
        }
        
        // Build the body of `actor_enumeration`
        {
            var mainBlock = actorEnumeration.Function.AppendBasicBlock("entry");
            builder.PositionAtEnd(mainBlock);

            var iter = builder.BuildAlloca(Current.Libs.ActorIterator, "iterator");

            builder.BuildCall2(Current.Libs.GetActorIterator, [iter]);
            
            // ReSharper disable InconsistentNaming
            var loop_test = actorEnumeration.Function.AppendBasicBlock("loop_test");
            var loop_body = actorEnumeration.Function.AppendBasicBlock("loop_body");
            // ReSharper restore InconsistentNaming
            
            var exit = actorEnumeration.Function.AppendBasicBlock("exit");

            builder.BuildBr(loop_test); // entry -> loop_test
            builder.PositionAtEnd(loop_test);
            var actor = builder.BuildCall2(Current.Libs.NextActor, [iter], "actor");
            var nullptr = LLVMValueRef.CreateConstPointerNull(Current.Libs.ActorStruct.Pointer);
            var condition = builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, actor, nullptr, "condition");
            builder.BuildCondBr(condition, loop_body, exit);
            
            builder.PositionAtEnd(loop_body);
            var charPtr = LLVMTypeRef.CreatePointer(context.Int8Type, 0);
            var actorNameField = builder.BuildStructGEP2(Current.Libs.ActorStruct, actor, 0);
            var actorName = builder.BuildLoad2(charPtr, actorNameField, "actor_name");
            builder.BuildPrintLine(actorName);
            
            builder.BuildBr(loop_test);
            
            builder.PositionAtEnd(exit);
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
    
    private static List<LLVMMethod> GenerateOnStartMethods(SyntaxTree tree)
    {
        List<LLVMMethod> list = [];
        
        foreach (var target in tree.GetAllTargets())
        {
            var count = 0;
            foreach (var script in target.Blocks)
                if(script.Entry is OnStart)
                    list.Add(GenMethodCalls.GenerateMethod(target, script, $"OnStart_{count++}"));
        }

        return list;
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