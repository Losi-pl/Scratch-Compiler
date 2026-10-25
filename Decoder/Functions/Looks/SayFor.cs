namespace ScratchCompiler.Decoder.Functions.Looks;

public class SayFor(Argument text, FloatArgument time) : Function
{
    public const string ScratchCode = "looks_sayforsecs";

    public Argument Text => text;
    public FloatArgument Time => time;
    
    public override string ToString() => $"Say {text} for {time} secs;";
}