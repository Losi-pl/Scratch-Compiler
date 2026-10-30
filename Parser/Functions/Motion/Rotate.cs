using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Motion;

public class Rotate : Function
{
    public const string ScratchLeftCode = "motion_turnleft";
    public const string ScratchRightCode = "motion_turnright";

    public FloatArgument Degrees { get; }
    public TurnDirection Direction { get; }

    private Rotate(FloatArgument degrees, TurnDirection direction) => (Degrees, Direction) = (degrees, direction);
    
    public override string ToString() => $"Rotate {Degrees} degrees to the {Direction.ToString().ToLower()};";
    
    [FunctionParser(ScratchLeftCode)][FunctionParser(ScratchRightCode)] // ReSharper disable once UnusedMember.Local
    private static Rotate Parse(JsonObject json, JsonObject list, Target _, string opcode) {
        var degrees = json.GetFloatInput(list, "DEGREES");
        return new Rotate(degrees, opcode == ScratchLeftCode ? TurnDirection.Left : TurnDirection.Right);
    }
    
    public enum TurnDirection : byte
    {
        Left, Right
    }
}