using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Motion;

public class MoveBySteps : Function
{
    public const string ScratchCode = "motion_movesteps";

    public FloatArgument Steps { get; }

    private MoveBySteps(FloatArgument steps) => Steps = steps;
    
    public override string ToString() => $"Move {Steps} steps forward;";
    
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static MoveBySteps Parse(JsonObject json, JsonObject list, Target _)
    {
        var steps = json.GetFloatInput(list, "STEPS");
        return new MoveBySteps(steps);
    }
}