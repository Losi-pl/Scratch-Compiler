using Humanizer;

namespace ScratchCompiler.Parser.Operations.Sensing;

public class IsTouching(IsTouching.Targets target) : LogicArgument
{
    public const string ScratchCode = "sensing_touchingobject";

    public Targets Target => target;

    public override string ToString() => $"{{Is actor touching {target.Humanize(LetterCasing.LowerCase)}}}";

    public enum Targets : byte
    {
        Mouse, Edge
    }
}