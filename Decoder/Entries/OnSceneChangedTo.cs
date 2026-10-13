namespace ScratchCompiler.Decoder.Entries;

public class OnSceneChangedTo : CodeEntry
{
    public const string ScratchCode = "event_whenbackdropswitchesto";

    public Scene TargetScene { get; }

    public OnSceneChangedTo(Scene targetScene) => TargetScene = targetScene;
}