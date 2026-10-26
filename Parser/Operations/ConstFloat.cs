using System.Globalization;

namespace ScratchCompiler.Parser.Operations;

public class ConstFloat(float @const) : FloatArgument, IBracketedString, IConstArgument
{
    public float Const => @const;

    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets) => useBrackets ? $"[{@const.ToString(CultureInfo.InvariantCulture)}]" : @const.ToString(CultureInfo.InvariantCulture);
}