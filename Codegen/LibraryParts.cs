using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;

namespace ScratchCompiler.Codegen;

internal class LibraryParts
{

    #region System

        public readonly LLVMMethod Puts;
        public readonly LLVMMethod Malloc;
        public readonly LLVMMethod AlignedMalloc;
        public readonly LLVMMethod ExitApp;
        public readonly LLVMTypeRef PointerType;
        public readonly LLVMValueRef Nullptr;
        public readonly LLVMAttributeRef HotAttribute;
        public readonly LLVMMethod RunApp;
        public readonly LLVMMethod InitWindow;
        public readonly LLVMMethod GlobalTimer;

    #endregion

    #region Scratch

        public readonly LLVMMethod ScratchInit;
        public readonly LLVMMethod CombineStrings3;
        public readonly LLVMMethod CastDoubleToString;

        public readonly StructType ActorStruct;
        public readonly StructType ScriptHolder;
        public readonly StructType ScriptEvents;
        public readonly StructType ListHandler;
        
        public readonly LLVMMethod RegisterActor;
        
        public readonly LLVMMethod GetActorIterator;
        public readonly LLVMMethod ConstructActor;
        public readonly LLVMMethod GetActorCount;
        public readonly LLVMMethod GetActors;
        public readonly LLVMMethod IterateNext;
        
        public readonly LLVMMethod ConstructScript;
        public readonly LLVMMethod RunScript;

        public readonly LLVMMethod CreateCostumeStorage;
        public readonly LLVMMethod LoadCostumeFromData;
        public readonly LLVMMethod LoadCostumeFromSvg;
        
    #endregion
    
    #region Motion 
    // ReSharper disable InconsistentNaming
    
        public readonly LLVMMethod MotionSetX;
        public readonly LLVMMethod MotionSetY;
        public readonly LLVMMethod MotionPointInDirection;
        
        public readonly LLVMMethod MotionMoveToXY;
        public readonly LLVMMethod MotionMoveToRandom;
        public readonly LLVMMethod MotionBeginGlideTo;
        public readonly LLVMMethod MotionGlideTo;
    
    // ReSharper restore InconsistentNaming
    #endregion

    #region Looks

        public readonly LLVMMethod LooksSay;
        public readonly LLVMMethod LooksThink;

    #endregion

    #region Operators

        public readonly LLVMMethod OperatorJoin;

    #endregion

    #region Sensing

        public readonly LLVMMethod SensingTimer;
        public readonly LLVMMethod SensingResetTimer;

    #endregion
    
    public LibraryParts(CodegenTools tools)
    {
        PointerType = tools.Context.CreatePointerType(0);
        Nullptr = LLVMValueRef.CreateConstPointerNull(PointerType);
        Puts = LLVMMethod.CreateMethod(tools.Context.VoidType, "puts", [LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0)]);
        Malloc = LLVMMethod.CreateMethod(PointerType, "malloc", [tools.Context.Int64Type]);
        AlignedMalloc = LLVMMethod.CreateMethod(PointerType, "_aligned_malloc", [tools.Context.Int64Type, tools.Context.Int64Type]);
        ExitApp = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "_exit", [Generator.Current.Context.Int32Type]);
        RunApp = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "start_app", []);
        InitWindow = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "create_window", [PointerType]);
        
        var charPtr = LLVMTypeRef.CreatePointer(tools.Context.Int8Type, 0);
        
        {
            ListHandler = StructType.CreateListHandler();
            ScriptEvents = StructType.Create(new[] {
                PointerType,                            // Script update's
                PointerType                             // Run OnStart's
            }, false, "Scratch::Script::Events");
            ScriptHolder = StructType.Create(new[] {
                Generator.Current.Context.Int32Type,    // uint script_point
                Generator.Current.Context.Int32Type,    // uint stack_size
                PointerType                             // void* method_stack
            }, false, "Scratch::Script");
            ActorStruct = StructType.Create(new[] {
                charPtr,                                // #0 char* name
                Generator.Current.Context.DoubleType,   // #1 double pos_x
                Generator.Current.Context.DoubleType,   // #2 double pos_y
                ScriptEvents.Pointer,                   // #3 Events* script_events
                ScriptHolder.Pointer,                   // #4 Script* scripts
                Generator.Current.Context.Int32Type,    // #5 uint script_count
                Generator.Current.Context.Int32Type,    // #6 uint current_costume
                Generator.Current.Context.DoubleType,   // #7 double scale
                PointerType,                            // #8 Costumes[] costumes
                Generator.Current.Context.DoubleType,   // #9 double direction
            }, false, "Scratch::Actor");
            
            GetActorIterator = LLVMMethod.CreateMethod(tools.Context.VoidType, "?begin_actor_iteration@@YA?AV?$_List_iterator@V?$_List_val@U?$_List_simple_types@PEAX@std@@@std@@@std@@XZ", [ListHandler.Pointer]);
            IterateNext = LLVMMethod.CreateMethod(ActorStruct.Pointer, "get_iteration_next", [ListHandler.Pointer, PointerType, Generator.Current.Context.Int1Type]);
            ConstructScript = LLVMMethod.CreateMethod(tools.Context.VoidType, "??0Script@@QEAA@I@Z", [ScriptHolder.Pointer, Generator.Current.Context.Int32Type]);
            RunScript = LLVMMethod.CreateMethod(tools.Context.VoidType, "run_actor_script", [ScriptHolder.Pointer, Generator.Current.Context.Int32Type]);
            RegisterActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "register_new_actor", [ActorStruct.Pointer]);
            GlobalTimer  = LLVMMethod.CreateMethod(Generator.Current.Context.DoubleType, "global_timer", []);
            GetActorCount = LLVMMethod.CreateMethod(tools.Context.Int32Type, "actor_count", []);
            GetActors = LLVMMethod.CreateMethod(PointerType, "get_actors", []);
            ConstructActor = LLVMMethod.CreateMethod(tools.Context.VoidType, "construct_actor", ActorStruct.Pointer, charPtr, 
                ScriptEvents.Pointer, tools.Context.DoubleType, tools.Context.DoubleType, tools.Context.Int32Type, 
                PointerType, tools.Context.Int32Type, tools.Context.DoubleType, tools.Context.DoubleType);
            
            MotionSetX = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_x@Actor@@QEAAXN@Z", [ActorStruct.Pointer, tools.Context.DoubleType]);
            MotionSetY = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_y@Actor@@QEAAXN@Z", [ActorStruct.Pointer, tools.Context.DoubleType]);
            MotionPointInDirection = LLVMMethod.CreateMethod(tools.Context.VoidType, "?set_direction@Actor@@QEAAXN@Z", [ActorStruct.Pointer, tools.Context.DoubleType]);
            MotionMoveToXY = LLVMMethod.CreateMethod(tools.Context.VoidType, "motion_goto_xy", [ActorStruct.Pointer, tools.Context.DoubleType, tools.Context.DoubleType]);
            MotionMoveToRandom = LLVMMethod.CreateMethod(tools.Context.VoidType, "motion_goto_random", [ActorStruct.Pointer]);
            MotionBeginGlideTo = LLVMMethod.CreateMethod(tools.Context.VoidType, "motion_begin_glide_to", [ActorStruct.Pointer, PointerType, tools.Context.DoubleType, tools.Context.DoubleType, tools.Context.DoubleType]);
            MotionGlideTo = LLVMMethod.CreateMethod(tools.Context.Int1Type, "motion_glide_to", [ActorStruct.Pointer, PointerType]);
            LooksSay = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_say", [charPtr, charPtr]);
            LooksThink = LLVMMethod.CreateMethod(Generator.Current.Context.VoidType, "looks_think", [charPtr, charPtr]);
            OperatorJoin = LLVMMethod.CreateMethod(charPtr, "operator_join", [charPtr, charPtr]);
            SensingTimer  = LLVMMethod.CreateMethod(Generator.Current.Context.DoubleType, "sensing_timer", []);
            SensingResetTimer  = LLVMMethod.CreateMethod(Generator.Current.Context.DoubleType, "sensing_reset_timer", []);
            
            CreateCostumeStorage = LLVMMethod.CreateMethod(PointerType, "create_costume_storage", [tools.Context.Int32Type]);
            LoadCostumeFromData = LLVMMethod.CreateMethod(tools.Context.VoidType, "load_costume_data", [PointerType, tools.Context.Int32Type, PointerType, tools.Context.Int64Type, charPtr]);
            LoadCostumeFromSvg = LLVMMethod.CreateMethod(tools.Context.VoidType, "load_costume_svg", [PointerType, tools.Context.Int32Type, charPtr, charPtr]);
        }
        
        {
            ScratchInit = LLVMMethod.CreateMethod(tools.Context.VoidType, "scratch_init", []);
            CombineStrings3 = LLVMMethod.CreateMethod(charPtr, "combineStrings_3", [charPtr, charPtr, charPtr]);
            CastDoubleToString = LLVMMethod.CreateMethod(charPtr, "cast_double_to_string", [Generator.Current.Context.DoubleType]);
        }

        HotAttribute = Generator.Current.Context.CreateEnumAttribute("hot", 0);
    }
}