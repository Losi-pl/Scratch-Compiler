using Humanizer;

namespace ScratchCompiler.Parser.Operations;

public class UnknownIntArgument(string opcode, int @const) : IntArgument
{
    public float Const => @const;
    
    public override string ToString() => $"[{@const}]";
}