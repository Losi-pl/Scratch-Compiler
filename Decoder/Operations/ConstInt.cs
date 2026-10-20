namespace ScratchCompiler.Decoder.Operations;

public class ConstInt(int @const) : FloatArgument, IConstArgument
{
    public int Const => @const;

    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets) => useBrackets ? $"[{@const}]" : @const.ToString();
}