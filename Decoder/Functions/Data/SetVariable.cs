using System.Collections.Frozen;
using Humanizer;
using OneOf;

namespace ScratchCompiler.Decoder.Functions.Data;

public class SetVariable : Function
{
    public const string ScratchCode = "data_setvariableto";
    
    public const string ScratchSetXCode = "motion_setx";
    public const string ScratchSetYCode = "motion_sety";
    public const string ScratchRotationCode = "motion_pointindirection";
    public const string ScratchSizeCode = "looks_setsizeto";
    public const string ScratchEffectCode = "looks_seteffectto";

    public static readonly FrozenDictionary<string, (string Value, SysVariables Type)> SystemVariables =
        new KeyValuePair<string, (string Value, SysVariables Type)>[] {
            new(ScratchRotationCode, ("DIRECTION", SysVariables.Rotation)),
            new(ScratchSetXCode,     ("X", SysVariables.XPosition)),
            new(ScratchSetYCode,     ("Y", SysVariables.YPosition)),
            new(ScratchSizeCode,     ("SIZE", SysVariables.Size)),
        }.ToFrozenDictionary();

    public static readonly FrozenDictionary<string, SysVariables> GraphicEffects =
        new KeyValuePair<string, SysVariables>[]
        {
            new("Color".ToUpper(), SysVariables.Color),
            new("Fisheye".ToUpper(), SysVariables.Fisheye),
            new("Whirl".ToUpper(), SysVariables.Whirl),
            new("Pixelate".ToUpper(), SysVariables.Pixelate),
            new("Mosaic".ToUpper(), SysVariables.Mosaic),
            new("Brightness".ToUpper(), SysVariables.Brightness),
            new("Ghost".ToUpper(), SysVariables.Ghost),
        }.ToFrozenDictionary();
    
    private readonly OneOf<SysVariables, Variable> _destination;
    private readonly Argument _value;
    
    public SetVariable(Variable destination, Argument value) => (_destination, _value) = (destination, value);
    public SetVariable(SysVariables destination, Argument value) => (_destination, _value) = (destination, value);

    public override string ToString() => $"Set {{{
        _destination switch {
            {IsT0:true, AsT0: var t} => $"{t.ToSystemMarker()}: {t.Humanize(LetterCasing.Title)}",
            {IsT1:true, AsT1: var t} => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}} to {_value};";
    
    public enum SysVariables : byte
    {
        XPosition = 0, YPosition = 1, Rotation = 2, Size = 3,                                       // Standard
        Color = 4, Fisheye = 5, Whirl = 6, Pixelate = 7, Mosaic = 8, Brightness = 9, Ghost = 10,    // Graphic Effects
    }
}