namespace ScratchCompiler.Decoder.Operations;

public class Modulo(FloatArgument dividend, FloatArgument divisor) : FloatArgument
{
    public const string ScratchName = "operator_mod";
    
    public FloatArgument Dividend => dividend;
    public FloatArgument Divisor => divisor;

    public override string ToString() => $"({
        (dividend is IConstArgument and IBracketedString div ? div.ToString(false) : dividend.ToString())} mod {
        (divisor is IConstArgument and IBracketedString dev ? dev.ToString(false) : divisor.ToString())})";
}