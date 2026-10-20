namespace ScratchCompiler.Decoder.Operations;

public class Round(FloatArgument input) : FloatArgument
{
    public const string ScratchCode = "operator_round";
    
    public FloatArgument Input => input;

    public override string ToString() => $"{{Round {input}}}";
}