namespace ScratchCompiler.Decoder.Entries;

public class OnGraterThan : CodeEntry
{
    public const string ScratchCode = "event_whengreaterthan";

    private InputType Source { get; }
    private FloatArgument Threshold { get; }

    internal OnGraterThan(InputType source, FloatArgument threshold) => (Source, Threshold) = (source, threshold);
    
    public override string ToString() => $"When {{{Source}}} is grater than {Threshold}:";
    
    public enum InputType: byte { Loudness, Timer }
}