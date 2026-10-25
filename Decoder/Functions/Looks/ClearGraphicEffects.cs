namespace ScratchCompiler.Decoder.Functions.Looks;

public class ClearGraphicEffects : Function
{
    public const string ScratchCode = "looks_cleargraphiceffects";

    public static readonly ClearGraphicEffects Instance = new();
    
    public override string ToString() => "Clear graphic effects;";
}