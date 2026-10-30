using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Humanizer;
using OneOf;

namespace ScratchCompiler.Parser.Functions.Data;

public class SetVariable : Function
{
    public const string ScratchCode = "data_setvariableto";
    
    public const string ScratchSetXCode = "motion_setx";
    public const string ScratchSetYCode = "motion_sety";
    public const string ScratchRotationCode = "motion_pointindirection";
    public const string ScratchSizeCode = "looks_setsizeto";
    public const string ScratchVolumeCode = "sound_setvolumeto";
    
    public const string ScratchGraphicCode = "looks_seteffectto";
    public const string ScratchSoundCode = "sound_seteffectto";

    public static readonly FrozenDictionary<string, (string Value, SysVariables Type)> SystemVariables =
        new KeyValuePair<string, (string Value, SysVariables Type)>[] {
            new(ScratchRotationCode, ("DIRECTION", SysVariables.Rotation)),
            new(ScratchSetXCode,     ("X", SysVariables.XPosition)),
            new(ScratchSetYCode,     ("Y", SysVariables.YPosition)),
            new(ScratchSizeCode,     ("SIZE", SysVariables.Size)),
            new(ScratchVolumeCode,   ("VOLUME", SysVariables.Volume)),
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
    
    private SetVariable(Variable destination, Argument value) => (_destination, _value) = (destination, value);
    private SetVariable(SysVariables destination, Argument value) => (_destination, _value) = (destination, value);

    public override string ToString() => $"Set {{{
        _destination switch {
            {IsT0:true, AsT0: var t} => $"{t.ToSystemMarker()}: {t.Humanize(LetterCasing.Title)}",
            {IsT1:true, AsT1: var t} => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}} to {_value};";
    
    // ReSharper disable once UnusedMember.Local
    [FunctionParser(ScratchSetXCode)]     [FunctionParser(ScratchSetYCode)]
    [FunctionParser(ScratchRotationCode)] [FunctionParser(ScratchSizeCode)]
    [FunctionParser(ScratchVolumeCode)]
    private static SetVariable ParseSystem(JsonObject json, JsonObject list, Target _, string opcode) {
        var value = json.GetGeneralInput(list, SystemVariables[opcode].Value);
        return new SetVariable(SystemVariables[opcode].Type, value);
    }
    
    [FunctionParser(ScratchSoundCode)] // ReSharper disable once UnusedMember.Local
    private static SetVariable ParseSounds(JsonObject json, JsonObject list, Target _) {
        var dest = json.GetField("EFFECT");
        var value = json.GetFloatInput(list, "VALUE");
        return new SetVariable(dest switch
        {
            "PITCH" => SysVariables.Pitch,
            "PAN" => SysVariables.PanStereo,
            _ => throw new NotSupportedException($"Unknown value type \"{dest}\"")
        }, value);
    }
    
    [FunctionParser(ScratchGraphicCode)] // ReSharper disable once UnusedMember.Local
    private static SetVariable ParseGraphics(JsonObject json, JsonObject list, Target _) {
        var effect = json.GetField("EFFECT");
        var value = json.GetGeneralInput(list, "VALUE");
        return new SetVariable(GraphicEffects[effect], value);
    }
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SetVariable Parse(JsonObject json, JsonObject list, Target _) {
        var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
        var variable = SyntaxTree.Current.project.Variables.FindByScratchCode(@ref);
        return new SetVariable(variable ?? throw new KeyNotFoundException(), json.GetGeneralInput(list, "VALUE"));
    }
    
    public enum SysVariables : byte
    {
        XPosition, YPosition, Rotation, Size, Volume,                  // Standard
        Color, Fisheye, Whirl, Pixelate, Mosaic, Brightness, Ghost,    // Graphic Effects
        Pitch, PanStereo                                               // Sound Effects
    }
}