namespace ScratchCompiler.Decoder.Functions.Motion;

public class BounceFromEdge : Function
{
    public const string ScratchCode = "motion_ifonedgebounce";

    public static readonly BounceFromEdge Instance = new();

    public override string ToString() => "If on edge, bounce;";
}