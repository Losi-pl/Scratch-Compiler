using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Humanizer;

namespace ScratchCompiler.Parser.Functions.Motion;

public class SetRotationStyle : Function
{
    public const string ScratchCode = "motion_setrotationstyle";
    public Modes Mode { get; }

    public static readonly FrozenDictionary<string, Modes> ModesMap =
        new Dictionary<string, Modes>
        {
            ["left-right"] = Modes.LeftRight,
            ["don't rotate"] = Modes.Locked,
            ["all around"] = Modes.Free,
        }.ToFrozenDictionary();

    private SetRotationStyle(Modes mode) => Mode = mode;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SetRotationStyle Parse(JsonObject json, JsonObject list, Target _) {
        var style = json.GetField("STYLE");
        return new SetRotationStyle(ModesMap[style]);
    }
    
    public override string ToString() => $"Set rotation style to {{{Mode.Humanize(LetterCasing.Title)}}};";

    public enum Modes : byte { Free, LeftRight, Locked }
}