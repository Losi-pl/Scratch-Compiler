namespace ScratchCompiler.Decoder.Functions.Looks;

public class ThinkFor(Argument text, FloatArgument time) : Function
{
    public const string ScratchCode = "looks_thinkforsecs";

    public Argument Text => text;
    public FloatArgument Time => time;
    
    public override string ToString() => $"Think {text} for {time} secs";
}