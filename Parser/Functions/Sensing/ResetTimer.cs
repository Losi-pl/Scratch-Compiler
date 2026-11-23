using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sensing;

public class ResetTimer : Function
{
    public const string ScratchCode = "sensing_resettimer";
    
    private static readonly ResetTimer _instance = new();

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static ResetTimer Parse(JsonObject _1, JsonObject _2, Target _3) => _instance;

    public override string ToString() => "Reset timer;";
}