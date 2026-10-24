using OneOf;

namespace ScratchCompiler.Decoder.Functions.Looks;

public class SwitchBackground(OneOf<SwitchBackground.Relative, Costume> destination) : Function
{
    public const string ScratchCode = "looks_switchbackdropto";
    public const string ScratchNextCode = "looks_nextbackdrop";

    public bool IsAbsolute => destination.IsT1;
    public bool IsRelative => destination.IsT0;

    public Relative AsRelative => destination.AsT0;
    public Costume AsAbsolute => destination.AsT1;
    
    public override string ToString() => destination switch
    {
        { IsT0: true, AsT0: var t} => $"{t} background;",
        { IsT1: true, AsT1: var t } => $"Swich bacground to {{{t.Name}}};",
        _ => throw new InvalidOperationException()
    };

    public enum Relative : byte
    {
        Next, Previous, Random
    }
}