using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Motion;

public class PointTowards : Function
{
    public const string ScratchCode = "motion_pointtowards";

    public override string ToString() => "Point towards [Mouse Position];";
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static PointTowards Parse(JsonObject json, JsonObject list, Parser.Target _) {
        var @ref = json.GetInputReference("TOWARDS", list);
        var mode = @ref.GetField("TOWARDS");
        if (mode != "_mouse_")
            throw new InvalidCastException();
        return new PointTowards();
    }
    
    public enum Target
    {
        Mouse
    }
}