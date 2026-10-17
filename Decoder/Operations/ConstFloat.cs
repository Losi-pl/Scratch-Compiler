namespace ScratchCompiler.Decoder.Operations;

public class ConstFloat(float @const) : FloatArgument
{
    public float Const => @const;

    public override string ToString() => $"[{@const}]";
}