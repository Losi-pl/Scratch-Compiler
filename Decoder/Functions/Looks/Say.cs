using Humanizer;

namespace ScratchCompiler.Decoder.Functions.Looks;

public class Say(string text) : Function
{
    public const string ScratchCode = "looks_say";

    public string Text => text;

    public override string ToString() => $"Say [{text.Truncate(20)}];";
}