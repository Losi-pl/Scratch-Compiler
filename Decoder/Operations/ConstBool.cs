namespace ScratchCompiler.Decoder.Operations;

public class ConstBool(bool @const) : FloatArgument, IBracketedString, IConstArgument
{
    public bool Const => @const;

    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets) => useBrackets ? $"[{@const}]" : @const.ToString();
}