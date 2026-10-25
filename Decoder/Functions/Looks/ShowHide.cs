namespace ScratchCompiler.Decoder.Functions.Looks;

public class ShowHide(bool visibility) : Function
{
    public const string ScratchShowCode = "looks_show";
    public const string ScratchHideCode = "looks_hide";

    public override string ToString() => visibility ? "Show;" : "Hide;";
}