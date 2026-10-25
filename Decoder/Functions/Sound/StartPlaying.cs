namespace ScratchCompiler.Decoder.Functions.Sound;

public class StartPlaying(Decoder.Sound sound) : Function
{
    public const string ScratchCode = "sound_play";

    public override string ToString() => $"Start playing {{{sound.Name}}} sound;";
}