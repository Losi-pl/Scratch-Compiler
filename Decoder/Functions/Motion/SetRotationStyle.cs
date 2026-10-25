using System.Collections.Frozen;
using Humanizer;

namespace ScratchCompiler.Decoder.Functions.Motion;

public class SetRotationStyle(SetRotationStyle.Modes mode) : Function
{
    public const string ScratchCode = "motion_setrotationstyle";
    public Modes Mode => mode;

    public static readonly FrozenDictionary<string, Modes> ModesMap =
        new Dictionary<string, Modes>
        {
            ["left-right"] = Modes.LeftRight,
            ["don't rotate"] = Modes.Locked,
            ["all around"] = Modes.Free,
        }.ToFrozenDictionary();

    public override string ToString() => $"Set rotation style to {{{mode.Humanize(LetterCasing.Title)}}};";

    public enum Modes : byte { Free, LeftRight, Locked }
}