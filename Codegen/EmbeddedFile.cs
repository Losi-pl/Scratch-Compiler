using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

public class EmbeddedFile(LLVMValueRef content, LLVMValueRef? size)
{
    public LLVMValueRef Content => content;
    public LLVMValueRef? Size => size;
}