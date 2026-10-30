using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sensing;

public class Ask : Function
{
    public const string ScratchCode = "sensing_askandwait";

    public Argument Question { get; }

    private Ask(Argument question) => Question = question;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static Ask Parse(JsonObject json, JsonObject list, Target _) {
        var question = json.GetStringInput(list, "QUESTION");
        return new Ask(question);
    }
    
    public override string ToString() => $"Ask {Question} and wait;";
}