namespace ScratchCompiler.Decoder.Operations;

public class ConstInt(int @const) : FloatArgument
{
    public int Const => @const;

    public override string ToString() => $"[{@const}]";
}