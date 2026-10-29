using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal static class Helpers
{
    public static LLVMValueRef BuildCall2(this in LLVMBuilderRef builder, LLVMMethod method, LLVMValueRef[] args, string name = "")
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(name);
        
        return builder.BuildCall2(method.FunctionType, method.Function, args, name);
    }
}