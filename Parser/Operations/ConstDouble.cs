using System.Globalization;

namespace ScratchCompiler.Parser.Operations;

public class ConstDouble(double @const) : FloatArgument, IBracketedString, IConstArgument
{
    public double Const => @const;

    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets) => useBrackets ? $"[{@const.ToString(CultureInfo.InvariantCulture)}]" : @const.ToString(CultureInfo.InvariantCulture);
}