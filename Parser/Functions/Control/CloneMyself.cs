using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Control;

public class CloneMyself : Function
{
    public const string ScratchCode = "control_create_clone_of";
    
    public static readonly CloneMyself Instance = new();

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static CloneMyself Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
    
    public override string ToString() => "Clone this actor;";
}