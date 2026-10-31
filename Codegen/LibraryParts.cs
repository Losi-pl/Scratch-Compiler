using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;

namespace ScratchCompiler.Codegen;

internal class LibraryParts
{
    public readonly LLVMMethod Puts;
    public readonly LLVMTypeRef PointerType;
    
    
    public readonly LLVMMethod CombineStrings2;
    public readonly LLVMMethod CombineStrings3;
    
    
    #region Motion

    

    #endregion

    #region Looks

    public readonly LLVMMethod LooksSay;
    public readonly LLVMMethod LooksThink;

    #endregion
    
    public LibraryParts(CodegenTools tools)
    {
        PointerType = tools.Context.CreatePointerType(0);
        Puts = LLVMMethod.CreateMethod(tools.Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0)]);
        
        
        {
            var charPtr = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);
            CombineStrings2 = LLVMMethod.CreateMethod(charPtr, "combineStrings_2", [charPtr, charPtr]);
            CombineStrings3 = LLVMMethod.CreateMethod(charPtr, "combineStrings_3", [charPtr, charPtr, charPtr]);
        }

        var charPointer = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);

        LooksSay = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_say", [charPointer, charPointer]);
        LooksThink = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_think", [charPointer, charPointer]);
    }
}