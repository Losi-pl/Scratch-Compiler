using System.Collections.Immutable;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;

namespace ScratchCompiler.Codegen;

internal class LibraryParts
{
    public readonly LLVMMethod Puts;
    public readonly LLVMMethod Malloc;
    public readonly LLVMTypeRef PointerType;
    
    
    public readonly LLVMMethod ScratchInit;
    public readonly LLVMMethod RegisterActor;
    public readonly LLVMMethod CombineStrings2;
    public readonly LLVMMethod CombineStrings3;

    public readonly StructType ActorStruct;
    public readonly LLVMMethod ConstructActor;
    public readonly LLVMMethod ActorSetPosX;
    public readonly LLVMMethod ActorSetPosY;
    
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
        Malloc = LLVMMethod.CreateMethod(PointerType, "malloc", [tools.Context.Int64Type]);

        {
            var fields = new[]
            {
                Generator.Current.Context.FloatType,
                Generator.Current.Context.FloatType,
            };

            ActorStruct = StructType.Create(fields, false, "Actor");
            ConstructActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "??0Actor@@QEAA@MM@Z", 
                [ActorStruct.Pointer, tools.Context.FloatType, tools.Context.FloatType]);
            ActorSetPosX = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_x@Actor@@QEAAXM@Z", 
                [ActorStruct.Pointer, tools.Context.FloatType]);
            ActorSetPosY = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_y@Actor@@QEAAXM@Z", 
                [ActorStruct.Pointer, tools.Context.FloatType]);
        }
        
        {
            var charPtr = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);
            ScratchInit = LLVMMethod.CreateMethod(tools.Context.VoidType, "scratch_init", []);
            RegisterActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "register_new_actor", [ActorStruct.Pointer]);
            CombineStrings2 = LLVMMethod.CreateMethod(charPtr, "combineStrings_2", [charPtr, charPtr]);
            CombineStrings3 = LLVMMethod.CreateMethod(charPtr, "combineStrings_3", [charPtr, charPtr, charPtr]);
        }

        var charPointer = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);

        LooksSay = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_say", [charPointer, charPointer]);
        LooksThink = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_think", [charPointer, charPointer]);
    }
}