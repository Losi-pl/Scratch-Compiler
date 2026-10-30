using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Looks;

public class Think : Function
{
    public const string ScratchCode = "looks_think";

    public Argument Text { get; }
    
    private Think(Argument text) => Text = text;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static Think Parse(JsonObject json, JsonObject list, Target _) {
        var text = json.GetStringInput(list, "MESSAGE");
        return new Think(text);
    }

    public override string ToString() => $"Think {Text};";
}