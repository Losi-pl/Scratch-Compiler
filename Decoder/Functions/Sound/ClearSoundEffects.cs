namespace ScratchCompiler.Decoder.Functions.Sound;

public class ClearSoundEffects : Function
{
    public const string ScratchCode = "sound_cleareffects";

    public static readonly ClearSoundEffects Instance = new();

    public override string ToString() => "Clear sound effects;";
}