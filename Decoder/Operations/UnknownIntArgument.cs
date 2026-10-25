using Humanizer;

namespace ScratchCompiler.Decoder.Operations;

public class UnknownIntArgument(string opcode, int @const) : IntArgument
{
    public float Const => @const;
    
    public override string ToString() => $"[{@const}]";
}