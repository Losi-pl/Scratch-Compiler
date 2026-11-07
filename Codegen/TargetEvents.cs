using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

internal class TargetEvents(LLVMMethod engine, LLVMMethod onStart, LLVMValueRef eventsStruct)
{
    public LLVMMethod Engine => engine;
    public LLVMMethod OnStart => onStart;
    public LLVMValueRef Struct => eventsStruct;
}