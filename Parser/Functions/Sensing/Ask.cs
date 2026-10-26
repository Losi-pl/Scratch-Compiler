namespace ScratchCompiler.Parser.Functions.Sensing;

public class Ask(Argument question) : Function
{
    public const string ScratchCode = "sensing_askandwait";

    public override string ToString() => $"Ask {question} and wait;";
}