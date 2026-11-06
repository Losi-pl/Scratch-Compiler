using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

internal class ScriptData(CodeBlock script, LLVMMethod method, bool defined, uint stackSize)
{
    public CodeBlock Script => script;
    public LLVMMethod Function => method;
    public bool IsDefined { get; set; } = defined;
    public uint StackSize { get; set; } = stackSize;
}