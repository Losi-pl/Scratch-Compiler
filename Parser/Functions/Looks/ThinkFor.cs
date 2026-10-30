using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Looks;

public class ThinkFor : Function
{
    public const string ScratchCode = "looks_thinkforsecs";

    public Argument Text { get; }
    public FloatArgument Time { get; }
    
    private ThinkFor(Argument text, FloatArgument time) => (Text, Time) = (text, time);
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static ThinkFor Parse(JsonObject json, JsonObject list, Target _) {
        var text = json.GetGeneralInput(list, "MESSAGE");
        var time = json.GetFloatInput(list, "SECS");
        return  new ThinkFor(text, time);
    }
    
    public override string ToString() => $"Think {Text} for {Time} secs;";
}