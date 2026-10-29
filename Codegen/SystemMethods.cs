using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;

namespace ScratchCompiler.Codegen;

internal class SystemMethods
{
    // ReSharper disable InconsistentNaming
    public readonly LLVMMethod Puts;
    public readonly CharPointer ENDL;
    public readonly LLVMTypeRef PointerType;
    // ReSharper restore InconsistentNaming
    
    
    public readonly LLVMMethod CombineStrings2;
    public readonly LLVMMethod CombineStrings3;
    
    
    public readonly CharPointer ActorIsSaying;

    public SystemMethods()
    {
        PointerType = Context.CreatePointerType(0);
        ENDL = Generator.Current.Builder.CreateGlobalConstString("\n", "std::endl");
        Puts = LLVMMethod.CreateMethod(Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(Context.Int8Type, 0)]);
        {
            var charPtr = LLVMTypeRef.CreatePointer(Context.Int8Type, 0);
            CombineStrings2 = LLVMMethod.CreateMethod(charPtr, "combineStrings_2", [charPtr, charPtr]);
            CombineStrings3 = LLVMMethod.CreateMethod(charPtr, "combineStrings_3", [charPtr, charPtr, charPtr]);
        }


        ActorIsSaying = Generator.Current.Builder.CreateGlobalConstString(" is saying: ", "scratch::target_is_saying");
    }

    private LLVMContextRef Context => Generator.Current.Context;
}