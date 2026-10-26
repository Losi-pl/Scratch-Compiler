using OneOf;

namespace ScratchCompiler.Parser.Functions.Looks;

public class SwitchCostume(OneOf<SwitchCostume.RelativeMode, Costume> direction) : Function
{
    public const string ScratchCode = "looks_switchcostumeto";
    public const string ScratchNextCode = "looks_nextcostume";
    
    public bool IsRelative => direction.IsT0;
    public RelativeMode Relative => direction.AsT0;
    public Costume Costume => direction.AsT1;

    public override string ToString() => direction.IsT0 ? "Next costume;" : $"Swich costume to {{{direction.AsT1.Name}}};";

    public enum RelativeMode : byte
    {
        Next
    }
}