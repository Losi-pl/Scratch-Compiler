using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Looks;

public class ShowHide : Function
{
    public const string ScratchShowCode = "looks_show";
    public const string ScratchHideCode = "looks_hide";
    
    public bool Visibility { get; }
    
    private ShowHide(bool visibility) => Visibility = visibility;

    [FunctionParser(ScratchShowCode)] [FunctionParser(ScratchHideCode)] // ReSharper disable once UnusedMember.Local
    private static ShowHide Parse(JsonObject _, JsonObject _1, Target _2, string opcode) => new(opcode == ScratchShowCode);
    
    public override string ToString() => Visibility ? "Show;" : "Hide;";
}