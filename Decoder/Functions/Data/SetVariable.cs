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

    public static readonly FrozenDictionary<string, (string Value, SysVariables Type)> SystemVariables =
        new KeyValuePair<string, (string Value, SysVariables Type)>[] {
            new(ScratchRotationCode, ("DIRECTION", SysVariables.Rotation)),
            new(ScratchSetXCode,     ("X", SysVariables.XPosition)),
            new(ScratchSetYCode,     ("Y", SysVariables.YPosition)),
        }.ToFrozenDictionary();
    
    private readonly OneOf<SysVariables, Variable> _destination;
    private readonly float _value;
    
    public SetVariable(Variable destination, float value) => (_destination, _value) = (destination, value);
    public SetVariable(SysVariables destination, float value) => (_destination, _value) = (destination, value);

    public override string ToString() => $"Set {{{
        _destination switch {
            {IsT0:true, AsT0: var t} => "System: " + t.Humanize(LetterCasing.Title),
            {IsT1:true, AsT1: var t} => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}} to [{_value}];";
    
    public enum SysVariables : byte
    {
        XPosition, YPosition, Rotation
    }
}