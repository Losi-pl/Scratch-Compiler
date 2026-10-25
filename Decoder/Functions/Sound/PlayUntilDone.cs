namespace ScratchCompiler.Decoder.Functions.Sound;

public class PlayUntilDone(Decoder.Sound sound) : Function
{
    public const string ScratchCode = "sound_playuntildone";

    public override string ToString() => $"Play {{{sound.Name}}} sound until done;";
}