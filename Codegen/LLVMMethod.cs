using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

// ReSharper disable once InconsistentNaming
internal record LLVMMethod(
    LLVMValueRef Function,
    LLVMTypeRef FunctionType,
    LLVMTypeRef ReturnType,
    LLVMTypeRef[] ParamTypes,
    bool IsVarArg
)
{
    internal static LLVMMethod CreateMethod(LLVMTypeRef returnType, string name, params LLVMTypeRef[] arguments)
    {
        var f = LLVMTypeRef.CreateFunction(returnType, arguments, false);
        var ft = Generator.Current.Module.AddFunction(name, f);

        return new(ft, f, returnType, arguments, false);
    }
    
}