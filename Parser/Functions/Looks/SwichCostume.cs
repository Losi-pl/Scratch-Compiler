using System.Text.Json.Nodes;
using OneOf;

namespace ScratchCompiler.Parser.Functions.Looks;

public class SwitchCostume : Function
{
    public const string ScratchCode = "looks_switchcostumeto";
    public const string ScratchNextCode = "looks_nextcostume";

    private readonly OneOf<RelativeMode, Costume> _direction;
    
    public bool IsRelative => _direction.IsT0;
    public RelativeMode Relative => _direction.AsT0;
    public Costume Costume => _direction.AsT1;

    private SwitchCostume(RelativeMode direction) => _direction = direction;
    private SwitchCostume(Costume target) => _direction = target;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SwitchCostume Parse(JsonObject json, JsonObject list, Target target) {
        var @ref = json.GetInputReference("COSTUME", list);
        var dest = @ref.GetField("COSTUME");
        var costume = target.Costumes.FirstOrDefault(c => c.Name == dest) ?? throw new KeyNotFoundException(dest);
        return new SwitchCostume(costume);
    }
    
    [FunctionParser(ScratchNextCode)] // ReSharper disable once UnusedMember.Local
    private static SwitchCostume ParseNext(JsonObject json, JsonObject list, Target target) => new(RelativeMode.Next);
    
    public override string ToString() => _direction.IsT0 ? "Next costume;" : $"Switch costume to {{{_direction.AsT1.Name}}};";

    public enum RelativeMode : byte
    {
        Next
    }
}