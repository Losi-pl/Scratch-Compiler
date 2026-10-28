using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

public static class Generator
{
    [ThreadStatic]
    private static (LLVMContextRef, LLVMModuleRef, LLVMBuilderRef)? _current;
    
    internal static (LLVMContextRef Context, LLVMModuleRef Module, LLVMBuilderRef Builder) Current => _current ?? throw new InvalidOperationException("No code generation was started");
    
    public static LLVMSharp.Module GenerateCode(this SyntaxTree tree, string? projectName = null)
    {
        var contextCs = new LLVMSharp.LLVMContext();
        var moduleCs = LLVMSharp.Module.Create(contextCs, projectName ?? "scratch-app");
        var builderCs = new LLVMSharp.IRBuilder(contextCs);

        var context = contextCs.Handle;
        var module = moduleCs.Handle;
        var builder = builderCs.Handle;
        
        _current = (context, module, builder);

        // Declare the `puts` function (int puts(char*))
        var putsRetTy = context.Int32Type;
        var putsParamTys = new[] { LLVMTypeRef.CreatePointer(context.Int8Type, 0) };
        var putsFnTy = LLVMTypeRef.CreateFunction(putsRetTy, putsParamTys);
        var putsFn = module.AddFunction("puts", putsFnTy);

        // Define the `main` function (void main())
        var mainRetTy = context.VoidType;
        var mainParamTys = new LLVMTypeRef[] { };
        var mainFnTy = LLVMTypeRef.CreateFunction(mainRetTy, mainParamTys);
        var mainFn = module.AddFunction("main", mainFnTy);
        var mainBlock = mainFn.AppendBasicBlock("entry");

        // Build the body of `main`: call puts("Hello, World!")
        builder.PositionAtEnd(mainBlock);
        var greeting = builder.BuildGlobalStringPtr("Hello, World!", "greeting");
        builder.BuildCall2(putsFnTy, putsFn, [greeting]);
        builder.BuildRetVoid();

        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        return moduleCs;
    }

    public static string Compile(this LLVMSharp.Module module, string path)
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

        var sysroot = Environment.GetEnvironmentVariable("llvm-sysroot");
        
        using var linkProcess = System.Diagnostics.Process.Start(
            // ReSharper disable UseCollectionExpression, StringLiteralTypo
            "lld-link", new[] { objectFile, 
                $"/out:{path}",
                "/defaultlib:libcmt.lib", 
                "/defaultlib:libvcruntime.lib",
                $"/libpath:{sysroot}/crt/lib/x86_64",
                $"/libpath:{sysroot}/sdk/lib/um/x86_64",
                $"/libpath:{sysroot}/sdk/lib/ucrt/x86_64" }
            // ReSharper restore UseCollectionExpression, StringLiteralTypo
        );
        linkProcess.WaitForExit();
        
        return objectFile;
    }

    public static void Execute(this LLVMSharp.Module module)
    {
        var engine = module.Handle.CreateExecutionEngine();
        var main = module.Handle.GetNamedFunction("main");
        engine.RunFunctionAsMain(main, 0, Array.Empty<string>(), Array.Empty<string>());
    }
}