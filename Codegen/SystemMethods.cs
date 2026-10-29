using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal class SystemMethods
{
    public readonly LLVMMethod Puts;

    public SystemMethods()
    {
        Puts = LLVMMethod.CreateMethod(Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(Context.Int8Type, 0)]);
    }

    private LLVMContextRef Context => Generator.Current.Context;
}