using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Looks;

public class ClearGraphicEffects : Function
{
    public const string ScratchCode = "looks_cleargraphiceffects";

    public static readonly ClearGraphicEffects Instance = new();
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static ClearGraphicEffects Parse(JsonObject _, JsonObject _2, Target _3) => Instance;
    
    public override string ToString() => "Clear graphic effects;";
}