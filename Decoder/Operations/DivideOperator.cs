using System.Globalization;

namespace ScratchCompiler.Decoder.Operations;

public class DivideOperator(FloatArgument a, FloatArgument b) : FloatArgument
{
    public const string ScratchCode = "operator_divide";
    private const string Null = "Null";
    
    public FloatArgument A => a;
    public FloatArgument B => b;

    public override string ToString() => $"({CustomNumerics(a)} / {CustomNumerics(b)})";

    private static string CustomNumerics(FloatArgument arg) => arg switch
    {
        ConstFloat fl => fl.Const.ToString(CultureInfo.InvariantCulture),
        ConstInt @in => @in.Const.ToString(),
        null => Null,
        _ => arg.ToString()
    } ?? Null;
}