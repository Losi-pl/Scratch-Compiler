namespace ScratchCompiler.Parser.Functions.Control;

public class Wait(FloatArgument delay) : Function
{
    public const string ScratchCode = "control_wait";

    public override string ToString() => $"Wait for {delay} seconds;";
}