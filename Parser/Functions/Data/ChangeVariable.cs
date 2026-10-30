using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Humanizer;
using OneOf;
using SysVariables = ScratchCompiler.Parser.Functions.Data.SetVariable.SysVariables;

namespace ScratchCompiler.Parser.Functions.Data;

public class ChangeVariable : Function
{
    public const string ScratchCode = "data_changevariableby";
    
    public const string ScratchXCode = "motion_changexby";
    public const string ScratchYCode = "motion_changeyby";
    public const string ScratchSizeCode = "looks_changesizeby";
    public const string ScratchVolumeCode = "sound_changevolumeby";

    public const string ScratchGraphicCode = "looks_changeeffectby";
    public const string ScratchSoundCode = "sound_changeeffectby";

    public static readonly FrozenDictionary<string, (string Value, SysVariables Type)> SystemVariables =
        new KeyValuePair<string, (string Value, SysVariables Type)>[] {
            new(ScratchXCode,      ("DX", SysVariables.XPosition)),
            new(ScratchYCode,      ("DY", SysVariables.YPosition)),
            new(ScratchSizeCode,   ("CHANGE", SysVariables.Size)),
            new(ScratchVolumeCode, ("VOLUME", SysVariables.Volume))
        }.ToFrozenDictionary();
    
    private readonly OneOf<SysVariables, Variable> _destination;
    private readonly Argument _value;
    
    private ChangeVariable(Variable destination, Argument value) => (_destination, _value) = (destination, value);
    private ChangeVariable(SysVariables destination, Argument value) => (_destination, _value) = (destination, value);

    // ReSharper disable once UnusedMember.Local
    [FunctionParser(ScratchXCode)] [FunctionParser(ScratchYCode)]
    [FunctionParser(ScratchSizeCode)] [FunctionParser(ScratchVolumeCode)] 
    private static ChangeVariable ParseSystem(JsonObject json, JsonObject list, Target _, string opcode) {
        var value = json.GetGeneralInput(list, SystemVariables[opcode].Value);
        return new ChangeVariable(SystemVariables[opcode].Type, value);
    }
    
    [FunctionParser(ScratchSoundCode)] // ReSharper disable once UnusedMember.Local
    private static ChangeVariable ParseSounds(JsonObject json, JsonObject list, Target _) {
        var dest = json.GetField("EFFECT");
        var value = json.GetFloatInput(list, "VALUE");
        return new ChangeVariable(dest switch
        {
            "PITCH" => SysVariables.Pitch,
            "PAN" => SysVariables.PanStereo,
            _ => throw new NotSupportedException($"Unknown value type \"{dest}\"")
        }, value);
    }
    
    [FunctionParser(ScratchGraphicCode)] // ReSharper disable once UnusedMember.Local
    private static ChangeVariable ParseGraphics(JsonObject json, JsonObject list, Target _) {
        var effect = json.GetField("EFFECT");
        var change = json.GetGeneralInput(list, "CHANGE");
        return new ChangeVariable(SetVariable.GraphicEffects[effect], change);
    }
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static ChangeVariable Parse(JsonObject json, JsonObject list, Target _) {
        var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
        var variable = SyntaxTree.Current.project.Variables.FindByScratchCode(@ref);
        return new ChangeVariable(variable ?? throw new KeyNotFoundException(), json.GetGeneralInput(list, "VALUE"));
    }
    
    public override string ToString() => $"Change {{{
        _destination switch {
            {IsT0:true, AsT0: var t} => $"{t.ToSystemMarker()}: {t.Humanize(LetterCasing.Title)}",
            {IsT1:true, AsT1: var t} => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}} by {_value};";
}