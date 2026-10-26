namespace ScratchCompiler.Parser.Functions.Sound;

public class StartPlaying(Parser.Sound sound) : Function
{
    public const string ScratchCode = "sound_play";

    public override string ToString() => $"Start playing {{{sound.Name}}} sound;";
}