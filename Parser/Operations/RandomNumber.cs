namespace ScratchCompiler.Parser.Operations;

public class RandomNumber(FloatArgument from, FloatArgument to) : FloatArgument
{
    public const string ScratchCode = "operator_random";
    
    public FloatArgument From => from;
    public FloatArgument To => to;

    public override string ToString() => $"{{Random number from {from} to {to}}}";
}