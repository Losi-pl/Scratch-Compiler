namespace ScratchCompiler.Decoder.Entries;

public class OnGraterThan : CodeEntry
{
    public const string ScratchCode = "event_whengreaterthan";

    private InputType Source { get; }
    private float Threshold { get; } = 0;

    internal OnGraterThan(InputType source, float threshold) => (Source, Threshold) = (source, threshold);
    
    public enum InputType: byte { Loudness, Timer }
}