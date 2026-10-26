namespace ScratchCompiler.Parser.Functions.Control;

public class Stop(Stop.Targets target) : Function
{
    public const string ScratchCode = "control_stop";

    public Targets Target => target;

    public override string ToString() => target switch
    {
        Targets.All => "Stop all scripts;",
        Targets.This => "Stop this script;",
        Targets.Others => "Stop other scripts;",
        _ => throw new InvalidOperationException()
    };

    public enum Targets
    {
        All, This, Others
    }
}