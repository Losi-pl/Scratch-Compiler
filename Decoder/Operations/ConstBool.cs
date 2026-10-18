namespace ScratchCompiler.Decoder.Operations;

public class ConstBool(bool @const) : FloatArgument
{
    public bool Const => @const;

    public override string ToString() => $"[{@const}]";
}