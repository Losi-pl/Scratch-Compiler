using System.Text.Json.Nodes;
using ScratchCompiler.Parser.Functions.Motion;

namespace ScratchCompiler.Parser.Functions.Looks;

public class SayFor : Function
{
    public const string ScratchCode = "looks_sayforsecs";

    public Argument Text { get; }
    public FloatArgument Time { get; }
    
    private SayFor(Argument text, FloatArgument time) => (Text, Time) = (text, time);
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SayFor Parse(JsonObject json, JsonObject list, Target _) {
        var text = json.GetGeneralInput(list, "MESSAGE");
        var time = json.GetFloatInput(list, "SECS");
        return  new SayFor(text, time);
    }
    
    public override string ToString() => $"Say {Text} for {Time} secs;";
}