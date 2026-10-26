namespace ScratchCompiler.Parser.Entries;

public class OnActorClick : CodeEntry
{
    public const string ScratchCode = "event_whenthisspriteclicked";

    public override string ToString() => "When Actor is clicked:";
}