using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sound;

public class ClearSoundEffects : Function
{
    public const string ScratchCode = "sound_cleareffects";

    public static readonly ClearSoundEffects Instance = new();

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static ClearSoundEffects Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
    
    public override string ToString() => "Clear sound effects;";
}