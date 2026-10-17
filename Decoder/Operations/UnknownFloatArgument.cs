using Humanizer;

namespace ScratchCompiler.Decoder.Operations;

public class UnknownFloatArgument(string opcode, float @const) : FloatArgument
{
    public float Const => @const;
    
    public override string ToString() => $"[{@const}]";
}