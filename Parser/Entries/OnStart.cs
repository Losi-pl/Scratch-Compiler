namespace ScratchCompiler.Parser.Entries;

public class OnStart: CodeEntry
{
    public const string ScratchCode = "event_whenflagclicked";

    public override string ToString() => "When game is started:";
}