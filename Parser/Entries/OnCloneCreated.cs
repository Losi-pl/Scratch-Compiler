namespace ScratchCompiler.Parser.Entries;

public class OnCloneCreated : CodeEntry
{
    public const string ScratchCode = "control_start_as_clone";

    public override string ToString() => "When clone is created:";
}