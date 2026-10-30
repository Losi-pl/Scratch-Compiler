using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Control;

public class Stop : Function
{
    public const string ScratchCode = "control_stop";

    public Targets Target { get; }

    private Stop(Targets target) => Target = target;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static Stop Parse(JsonObject json, JsonObject list, Target _) {
        var sTarget = json.GetField("STOP_OPTION");
        return new Stop(sTarget switch
        {
            "all" => Targets.All,
            "this script" => Targets.This,
            "other scripts in sprite" => Targets.Others,
            _ => throw new KeyNotFoundException($"\"{sTarget}\" is unknown stop target")
        });
    }
    
    public override string ToString() => Target switch
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