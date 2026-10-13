namespace ScratchCompiler.Decoder.Entries;

public class OnBackgroundChangedTo(Costume targetBackground) : CodeEntry
{
    public const string ScratchCode = "event_whenbackdropswitchesto";

    public Costume TargetScene { get; } = targetBackground;
}