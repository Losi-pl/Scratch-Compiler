using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

partial class Generator
{
    private static LLVMMethod GenActorEnumerationBody(LLVMBuilderRef builder)
    {
        var lambdaType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [Current.Libs.PointerType, Current.Libs.PointerType]);
        var fun = LLVMMethod.CreateMethod(Current.Context.VoidType, "actor_enumeration", [Current.Libs.PointerType, Current.Libs.PointerType]);
        fun.Function.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeFunctionIndex, Current.Libs.HotAttribute);
        fun.Function.GetParam(0).Name = "lambda";
        fun.Function.GetParam(1).Name = "lambda_env";
        
        // ReSharper disable InconsistentNaming
        var mainBlock = fun.Function.AppendBasicBlock("entry");                                 // Label: Entry
        var loop_test = fun.Function.AppendBasicBlock("loop_test");             // Label: Loop test
        var loop_body = fun.Function.AppendBasicBlock("loop_body");             // Label: Loop body
        var exit = fun.Function.AppendBasicBlock("exit");                       // Label: Exit
        // ReSharper restore InconsistentNaming
        builder.PositionAtEnd(mainBlock);                                                             // entry:
        var iter = builder.BuildAlloca(Current.Libs.ListHandler, "iterator");        // Stackalloc iteration spot
        var actors = builder.BuildCall2(Current.Libs.GetActors, [], "actors");           // Get the list
        builder.BuildCall2(Current.Libs.GetActorIterator, [iter]);                                        // Get actor iterator
        builder.BuildBr(loop_test); builder.PositionAtEnd(loop_test);                                 // loop_test:
        var actor = builder.BuildCall2(Current.Libs.IterateNext,                              // Get next actor
            [iter, actors, LLVMValueRef.CreateConst(false)], "actor");                               // 
        var condition = builder.BuildICmp(                                                    // is {actor == null}
            LLVMIntPredicate.LLVMIntNE, actor, Current.Libs.Nullptr, "condition");    // ^^
        builder.BuildCondBr(condition, loop_body, exit);                                         // goto condition ? loop_body : exit
        builder.PositionAtEnd(loop_body);                                                             // loop_body:
        builder.BuildCall2(lambdaType, fun.Function.GetParam(0), [actor, fun.Function.GetParam(1)]);   // Lambda(actor)
        builder.BuildBr(loop_test);                                                                       // Loop back
        builder.PositionAtEnd(exit);                                                                  // exit:
        builder.BuildRetVoid();                                                                           // Return

        return fun;
    }

    private static LLVMMethod GenRunScriptUpdates()
    {
        var lambda = LLVMMethod.CreateMethod(Current.Context.VoidType, "run_actor_scripts", [Current.Libs.PointerType, Current.Libs.PointerType]);
        var engineType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [Current.Libs.PointerType]);
        var main = lambda.Function.AppendBasicBlock("main");
        var actor = lambda.Function.GetParam(0); actor.Name = "Actor";
        lambda.Function.GetParam(1).Name = "lambda_env";
        Current.Builder.PositionAtEnd(main);
        var builder = Current.Builder;
        
        var eventsPtr = builder.BuildStructGEP2(Current.Libs.ActorStruct, actor, 3);   // field: Actor.events
        var events = builder.BuildLoad2(Current.Libs.PointerType, eventsPtr, "events"); // Get events
        var updatePtr = builder.BuildStructGEP2(Current.Libs.ScriptEvents, events, 0); // field: events.update
        var update = builder.BuildLoad2(Current.Libs.PointerType, updatePtr, "update"); // Get events.update
        builder.BuildCall2(engineType, update, [actor]);                                                  // events.update(actor);
        builder.BuildRetVoid();                                                                           // Return
        
        return lambda;
    }
    
    private static LLVMMethod GenRunOnStartScripts()
    {
        var lambda = LLVMMethod.CreateMethod(Current.Context.VoidType, "run_onStart_scripts", [Current.Libs.PointerType, Current.Libs.PointerType]);
        var engineType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [Current.Libs.PointerType]);
        var main = lambda.Function.AppendBasicBlock("main");
        var actor = lambda.Function.GetParam(0); actor.Name = "Actor";
        lambda.Function.GetParam(1).Name = "lambda_env";
        Current.Builder.PositionAtEnd(main);
        var builder = Current.Builder;
        
        var eventsPtr = builder.BuildStructGEP2(Current.Libs.ActorStruct, actor, 3);   // field: Actor.events
        var events = builder.BuildLoad2(Current.Libs.PointerType, eventsPtr, "events"); // Get events
        var updatePtr = builder.BuildStructGEP2(Current.Libs.ScriptEvents, events, 1); // field: events.onStart
        var update = builder.BuildLoad2(Current.Libs.PointerType, updatePtr, "update"); // Get events.onStart
        builder.BuildCall2(engineType, update, [actor]);                                                  // events.onStart(actor);
        builder.BuildRetVoid();                                                                           // Return
        
        return lambda;
    }
}