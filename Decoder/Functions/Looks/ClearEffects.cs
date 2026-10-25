namespace ScratchCompiler.Decoder.Functions.Looks;

public class ClearEffects : Function
{
    public const string ScratchCode = "looks_cleargraphiceffects";

    public static readonly ClearEffects Instance = new();
    
    public override string ToString() => "Clear graphic effects;";
}