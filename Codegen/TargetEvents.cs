using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal class TargetEvents(LLVMMethod updates, LLVMMethod onStart, LLVMValueRef eventsStruct)
{
    public LLVMMethod Updates => updates;
    public LLVMMethod OnStart => onStart;
    public LLVMValueRef Struct => eventsStruct;
}