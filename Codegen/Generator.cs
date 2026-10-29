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
    [ThreadStatic]
    internal static CurrentTools Current;
    
    private static SystemMethods Std => Current.Methods;
    
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
        Current.Methods = new();
        MapActorNames(tree);

        Current.OnStartMethods = GenerateOnStartMethods(tree).ToImmutableArray();
        LLVMMethod main;

        { // Main Method
            var mainRetTy = context.VoidType;
            var mainParamTys = Array.Empty<LLVMTypeRef>();
            var mainFnTy = LLVMTypeRef.CreateFunction(mainRetTy, mainParamTys);
            var mainFn = module.AddFunction("main", mainFnTy);

            main = new(mainFn, mainFnTy, mainRetTy, mainParamTys, false);
        } // Main Method
        

        // Build the body of `main`: call puts("Hello, World!")
        var mainBlock = main.Function.AppendBasicBlock("entry");
        builder.PositionAtEnd(mainBlock);
        {
            var hello = (CharPointer)builder.BuildGlobalStringPtr("Hello, World!");
            var world = (CharPointer)builder.BuildGlobalStringPtr(" by Losi");
            
            var text = builder.BuildStrCombine(hello, world);
            builder.BuildPrintLine(text);
            builder.BuildFree(text);
        }
        foreach (var method in Current.OnStartMethods)
            builder.BuildCall2(method, []);
        builder.BuildRetVoid();

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
        
        var voidTy = Current.Context.VoidType;
        var @params = Array.Empty<LLVMTypeRef>();
        var funType = LLVMTypeRef.CreateFunction(voidTy, @params);
        
        foreach (var target in tree.GetAllTargets())
        {
            var count = 0;
            foreach (var script in target.Blocks)
            {
                if (script.Entry is not OnStart)
                    continue;

                var methodName = $"{target.Name}::OnStart_{count:000}";
                count++;
                
                var fun = Current.Module.AddFunction(methodName, funType);
                
                list.Add(new(fun, funType, voidTy, @params, false));
                
                var currentBlock = fun.AppendBasicBlock("entry");
                Current.Builder.PositionAtEnd(currentBlock);

                foreach (var method in script.Methods)
                {
                    var conStrCount = -1;
                    
                    switch (method)
                    {
                        case Say say:
                        {
                            CharPointer text;
                            if(say.Text is ConstString @const)
                                text = Current.Builder.BuildGlobalString(@const.Const, $"{methodName}::str_{++conStrCount:00}");
                            else
                                throw NotImplemented(say.Text);
                            
                            var message = Current.Builder.BuildStrCombine(Current.TargetNames[target], Current.Methods.ActorIsSaying, text);

                            Current.Builder.BuildPrintLine(message);
                            Current.Builder.BuildFree(message);
                        } break;
                        
                        default:
                            throw NotImplemented(method);
                    }
                }

                Current.Builder.BuildRetVoid();
            }
        }

        return list;
    }

    
    
    private static Exception NotImplemented(object element) => 
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