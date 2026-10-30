using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sound;

public class StopPlaying : Function
{
    public const string ScratchCode = "sound_stopallsounds";

    public static readonly StopPlaying Instance = new();
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static StopPlaying Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
    
    public override string ToString() => "Stop playing sounds;";
}