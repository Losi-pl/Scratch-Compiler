namespace ScratchCompiler.Decoder.Entries;

public class OnStageClick : CodeEntry
{
    public const string ScratchCode = "event_whenstageclicked";

    public override string ToString() => "When Stage is clicked:";
}