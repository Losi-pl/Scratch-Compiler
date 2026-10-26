using Humanizer;

namespace ScratchCompiler.Parser.Functions.Looks;

public class Think(Argument text) : Function
{
    public const string ScratchCode = "looks_think";

    public Argument Text => text;

    public override string ToString() => $"Think {text};";
}