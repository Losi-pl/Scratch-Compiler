using Humanizer;

namespace ScratchCompiler.Decoder.Functions.Looks;

public class Think(string text) : Function
{
    public const string ScratchCode = "looks_think";

    public string Text => text;

    public override string ToString() => $"Think [{text.Truncate(20)}];";
}