using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Control;

public class DestroyClone : Function
{
    public const string ScratchCode = "control_delete_this_clone";
    
    public static readonly DestroyClone Instance = new();

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static DestroyClone Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
    
    public override string ToString() => "Destroy this clone;";
}