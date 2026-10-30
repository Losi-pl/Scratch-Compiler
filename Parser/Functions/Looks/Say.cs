using System.Text.Json.Nodes;
using Humanizer;

namespace ScratchCompiler.Parser.Functions.Looks;

public class Say : Function
{
    public const string ScratchCode = "looks_say";

    public Argument Text { get; }

    private Say(Argument text) => Text = text;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static Say Parse(JsonObject json, JsonObject list, Target _) {
        var text = json.GetStringInput(list, "MESSAGE");
        return new Say(text);
    }
    
    public override string ToString() => $"Say {Text};";
}