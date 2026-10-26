namespace ScratchCompiler.Parser.Functions.Control;

public class WaitUntil(LogicArgument condition) : Function
{
    public const string ScratchCode = "control_wait_until";

    public override string ToString() => $"Wait until {condition} is true;";
}