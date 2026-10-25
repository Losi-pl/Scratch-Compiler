namespace ScratchCompiler.Decoder.Functions.Sound;

public class StopPlaying : Function
{
    public const string ScratchCode = "sound_stopallsounds";

    public static readonly StopPlaying Instance = new();
    
    public override string ToString() => "Stop playing sounds;";
}