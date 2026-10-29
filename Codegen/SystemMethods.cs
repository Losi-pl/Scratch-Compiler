using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal class SystemMethods
{
    // ReSharper disable InconsistentNaming
    public readonly LLVMMethod Puts;
    public LLVMValueRef ENDL;
    // ReSharper restore InconsistentNaming

    public SystemMethods()
    {
        Puts = LLVMMethod.CreateMethod(Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(Context.Int8Type, 0)]);
        ENDL = Generator.Current.Builder.CreateGlobalConstString("\n", "std::endl");
    }

    private LLVMContextRef Context => Generator.Current.Context;
}