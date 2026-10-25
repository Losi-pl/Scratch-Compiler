using System.Collections.Frozen;
using Humanizer;
using OneOf;
using SysVariables = ScratchCompiler.Decoder.Functions.Data.SetVariable.SysVariables;

namespace ScratchCompiler.Decoder.Functions.Data;

public class ChangeVariable : Function
{
    public const string ScratchCode = "data_changevariableby";
    
    public const string ScratchXCode = "motion_changexby";
    public const string ScratchYCode = "motion_changeyby";
    public const string ScratchSizeCode = "looks_changesizeby";

    public static readonly FrozenDictionary<string, (string Value, SysVariables Type)> SystemVariables =
        new KeyValuePair<string, (string Value, SysVariables Type)>[] {
            new(ScratchXCode,     ("DX", SysVariables.XPosition)),
            new(ScratchYCode,     ("DY", SysVariables.YPosition)),
            new(ScratchSizeCode,  ("CHANGE", SysVariables.Size))
        }.ToFrozenDictionary();
    
    private readonly OneOf<SysVariables, Variable> _destination;
    private readonly Argument _value;
    
    public ChangeVariable(Variable destination, Argument value) => (_destination, _value) = (destination, value);
    public ChangeVariable(SysVariables destination, Argument value) => (_destination, _value) = (destination, value);

    public override string ToString() => $"Change {{{
        _destination switch {
            {IsT0:true, AsT0: var t} => "System: " + t.Humanize(LetterCasing.Title),
            {IsT1:true, AsT1: var t} => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}} by {_value};";
}