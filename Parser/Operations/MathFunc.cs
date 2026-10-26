using System.Collections.Frozen;

namespace ScratchCompiler.Parser.Operations;

public class MathFunc(MathFunc.Operations mode, FloatArgument input) : FloatArgument
{
    public const string ScratchCode = "operator_mathop";

    public static readonly FrozenDictionary<string, Operations> OperationMap =
        new Dictionary<string, Operations>()
        {
            ["abs"] = Operations.Abs,
            ["floor"] = Operations.Floor,
            ["ceiling"] = Operations.Ceiling,
            ["sqrt"] = Operations.Sqrt,
            ["sin"] = Operations.Sin,
            ["cos"] = Operations.Cos,
            ["tan"] = Operations.Tan,
            ["asin"] = Operations.Asin,
            ["acos"] = Operations.Acos,
            ["atan"] = Operations.Atan,
            ["ln"] = Operations.Ln,
            ["log"] = Operations.Log,
            ["e ^"] = Operations.EToPower,
            ["10 ^"] = Operations.PowersOf10,
        }.ToFrozenDictionary();

    public override string ToString() => mode switch
    {
        Operations.EToPower => $"(e ^ {(input is IConstArgument and IBracketedString bs ? bs.ToString(false) : input.ToString())})",
        Operations.PowersOf10 => $"(10 ^ {(input is IConstArgument and IBracketedString bs ? bs.ToString(false) : input.ToString())})",
        _ => $"{mode}({(input is IBracketedString bs ? bs.ToString(false) : input)})"
    };

    public enum Operations : byte
    {
        Abs, Floor, Ceiling, Sqrt, Sin, Cos, Tan,
        Asin, Acos, Atan, Ln, Log, EToPower,
        PowersOf10
    }
}