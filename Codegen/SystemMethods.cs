using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal class SystemMethods
{
    // ReSharper disable InconsistentNaming
    public readonly LLVMMethod Puts;
    public readonly LLVMMethod Malloc;
    public readonly LLVMMethod Free;
    public readonly LLVMValueRef ENDL;
    public readonly LLVMTypeRef PointerType;
    // ReSharper restore InconsistentNaming
    
    
    public readonly LLVMMethod CombineStrings;

    public SystemMethods()
    {
        PointerType = Context.CreatePointerType(0);
        ENDL = Generator.Current.Builder.CreateGlobalConstString("\n", "std::endl");
        Puts = LLVMMethod.CreateMethod(Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(Context.Int8Type, 0)]);
        Malloc = LLVMMethod.CreateMethod(PointerType, "malloc", [Context.Int64Type]);
        Free = LLVMMethod.CreateMethod(Context.VoidType, "free", [PointerType]);
        {
            var charPtr = LLVMTypeRef.CreatePointer(Context.Int8Type, 0);
            CombineStrings = LLVMMethod.CreateMethod(charPtr, "combineStrings", [charPtr, charPtr]);
        }
    }

    private LLVMContextRef Context => Generator.Current.Context;
}