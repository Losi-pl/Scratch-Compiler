using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

public static class Generator
{
    public static LLVMSharp.Module GenerateCode(this SyntaxTree tree, string? projectName = null)
    {
        var contextCs = new LLVMSharp.LLVMContext();
        var moduleCs = LLVMSharp.Module.Create(contextCs, projectName ?? "scratch-app");
        var builderCs = new LLVMSharp.IRBuilder(contextCs);

        var context = contextCs.Handle;
        var module = moduleCs.Handle;
        var builder = builderCs.Handle;

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
        var message = builder.BuildGlobalStringPtr("Hello, World!");
        builder.BuildCall2(putsFnTy, putsFn, new LLVMValueRef[] { message }, "");
        builder.BuildRetVoid();

        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        Console.WriteLine(module.PrintToString());

        var triple = LLVMTargetRef.DefaultTriple;
        Console.WriteLine($"Targeting {triple}");

        return moduleCs;
    }
}