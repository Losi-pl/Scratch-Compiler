using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

partial class Generator
{
    private static void LocalActorEnumerationBody(LLVMBuilderRef builder, LLVMMethod fun)
    {
        fun.Function.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeFunctionIndex, Current.Libs.HotAttribute);
        var engineType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [Current.Libs.ActorStruct.Pointer]);
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
        builder.BuildBr(loop_test);                                                                       // entry -> loop_test
        builder.PositionAtEnd(loop_test);                                                             // loop_test:
        var actor = builder.BuildCall2(Current.Libs.IterateNext,                              // Get next actor
            [iter, actors, LLVMValueRef.CreateConst(false)], "actor");                               // ^^^
        var nullptr = LLVMValueRef.CreateConstPointerNull(Current.Libs.ActorStruct.Pointer);              // Const: nullptr
        var condition = builder.BuildICmp(                                                    // is {actor == null}
            LLVMIntPredicate.LLVMIntNE, actor, nullptr, "condition");                 // ^^
        builder.BuildCondBr(condition, loop_body, exit);                                         // goto condition ? loop_body : exit

        builder.PositionAtEnd(loop_body);                                                             // loop_body:
        var enginePtr = builder.BuildStructGEP2(                                              // field: Actor.engine_method
            Current.Libs.ActorStruct, actor, 3, "method*");                                  // ^^^
        var engine = builder.BuildLoad2(Current.Libs.PointerType, enginePtr, "engine"); // Get engine method
        builder.BuildCall2(engineType, engine, [actor]);                                                  // engine(actor);
        builder.BuildBr(loop_test);                                                                       // Loop back

        builder.PositionAtEnd(exit);                                                                  // exit:
        builder.BuildRetVoid();                                                                           // return;
    }
}