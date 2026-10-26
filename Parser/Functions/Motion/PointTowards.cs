namespace ScratchCompiler.Parser.Functions.Motion;

public class PointTowards : Function
{
    public const string ScratchCode = "motion_pointtowards";
    
    public enum Target
    {
        Mouse
    }

    public override string ToString() => "Point towards [Mouse Position];";
}