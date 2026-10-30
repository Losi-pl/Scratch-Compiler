using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Control;

public class Wait : Function
{
    public const string ScratchCode = "control_wait";

    public FloatArgument Delay { get; }
    
    private Wait(FloatArgument delay) => Delay = delay;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static Wait Parse(JsonObject json, JsonObject list, Target _) {
        var delay = json.GetFloatInput(list, "DURATION");
        return new Wait(delay);
    }
    
    public override string ToString() => $"Wait for {Delay} seconds;";
}