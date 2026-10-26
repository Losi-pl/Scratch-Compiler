using Humanizer;

namespace ScratchCompiler.Parser.Operations;

public class UnknownFloatArgument(string opcode, float @const) : FloatArgument
{
    public float Const => @const;
    
    public override string ToString() => $"[{@const}]";
}