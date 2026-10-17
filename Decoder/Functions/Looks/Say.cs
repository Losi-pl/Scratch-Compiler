using Humanizer;

namespace ScratchCompiler.Decoder.Functions.Looks;

public class Say(Argument text) : Function
{
    public const string ScratchCode = "looks_say";

    public Argument Text => text;

    public override string ToString() => $"Say {text};";
}