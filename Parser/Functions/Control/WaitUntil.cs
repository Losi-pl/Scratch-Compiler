using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Control;

public class WaitUntil : Function
{
    public const string ScratchCode = "control_wait_until";

    public LogicArgument Condition { get; }
    
    private WaitUntil(LogicArgument condition) => Condition = condition;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static WaitUntil Parse(JsonObject json, JsonObject list, Target _) {
        var predicate = json.GetLogicInput(list, "CONDITION");
        return new WaitUntil(predicate);
    }
    
    public override string ToString() => $"Wait until {Condition} is true;";
}