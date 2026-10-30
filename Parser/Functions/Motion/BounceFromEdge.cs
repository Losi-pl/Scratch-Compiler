using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Motion;

public class BounceFromEdge : Function
{
    internal const string ScratchCode = "motion_ifonedgebounce";

    public static readonly BounceFromEdge Instance = new();

    public override string ToString() => "If on edge, bounce;";

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static BounceFromEdge Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
}