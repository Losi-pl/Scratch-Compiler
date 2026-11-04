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
    public readonly LLVMMethod CombineStrings2;
    public readonly LLVMMethod CombineStrings3;

    public readonly StructType ActorStruct;
    public readonly StructType ScriptHolder;
    public readonly StructType ListHandler;
    
    public readonly LLVMMethod RegisterActor;
    
    public readonly LLVMMethod GetActorIterator;
    public readonly LLVMMethod ConstructActor;
    public readonly LLVMMethod GetActorCount;
    public readonly LLVMMethod GetActors;
    public readonly LLVMMethod IterateNext;
    
    public readonly LLVMMethod ConstructScript;

    public readonly LLVMAttributeRef HotAttribute;
    
    #region Motion

        public readonly LLVMMethod ActorSetPosX;
        public readonly LLVMMethod ActorSetPosY;

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
        
        var charPtr = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);

        {
            ListHandler = StructType.CreateListHandler();
            var scriptFields = new[]
            {
                Generator.Current.Context.Int32Type,    // uint script_point
                Generator.Current.Context.Int32Type,    // uint stack_size
                PointerType                             // void* method_stack
            };
            ScriptHolder = StructType.Create(scriptFields, false, "Scratch::Script");
            var actorFields = new[]
            {
                charPtr,                                // char* name
                Generator.Current.Context.FloatType,    // float pos_x
                Generator.Current.Context.FloatType,    // float pos_y
                PointerType,                            // void* scripts_engine
                ScriptHolder.Pointer,                   // Script* scripts
                Generator.Current.Context.Int32Type     // uint script_count
            };
            ActorStruct = StructType.Create(actorFields, false, "Scratch::Actor");
            
            GetActorIterator = LLVMMethod.CreateMethod(tools.Context.VoidType, "?begin_actor_iteration@@YA?AV?$_List_iterator@V?$_List_val@U?$_List_simple_types@PEAX@std@@@std@@@std@@XZ", [ListHandler.Pointer]);
            IterateNext = LLVMMethod.CreateMethod(ActorStruct.Pointer, "get_iteration_next", [ListHandler.Pointer, PointerType, Generator.Current.Context.Int1Type]);
            RegisterActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "register_new_actor", [ActorStruct.Pointer]);
            GetActorCount = LLVMMethod.CreateMethod(tools.Context.Int32Type, "actor_count", []);
            GetActors = LLVMMethod.CreateMethod(PointerType, "get_actors", []);
            
            ConstructScript = LLVMMethod.CreateMethod(tools.Context.VoidType, "??0Script@@QEAA@I@Z", [ScriptHolder.Pointer, Generator.Current.Context.Int32Type]);
            
            ConstructActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "??0Actor@@QEAA@PEADPEAXMMI@Z", [ActorStruct.Pointer, charPtr, PointerType, tools.Context.FloatType, tools.Context.FloatType, tools.Context.Int32Type]);
            ActorSetPosX = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_x@Actor@@QEAAXM@Z", [ActorStruct.Pointer, tools.Context.FloatType]);
            ActorSetPosY = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_y@Actor@@QEAAXM@Z", [ActorStruct.Pointer, tools.Context.FloatType]);
        }
        
        {
            ScratchInit = LLVMMethod.CreateMethod(tools.Context.VoidType, "scratch_init", []);
            CombineStrings2 = LLVMMethod.CreateMethod(charPtr, "combineStrings_2", [charPtr, charPtr]);
            CombineStrings3 = LLVMMethod.CreateMethod(charPtr, "combineStrings_3", [charPtr, charPtr, charPtr]);
        }

        LooksSay = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_say", [charPtr, charPtr]);
        LooksThink = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_think", [charPtr, charPtr]);

        HotAttribute = Generator.Current.Context.CreateEnumAttribute("hot", 0);
    }
}