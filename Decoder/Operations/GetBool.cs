using System.Collections.Frozen;
using Humanizer;
using OneOf;

namespace ScratchCompiler.Decoder.Operations;

public class GetBool : LogicArgument
{
    public const string ScratchOnlineCode = "sensing_online";
    
    private readonly OneOf<SystemSources, Variable> _source;
    
    public GetBool(SystemSources source) => _source = source;
    public GetBool(Variable source) => _source = source;

    public static readonly FrozenDictionary<string, SystemSources> SystemSourcesMap = 
        new Dictionary<string, SystemSources>
        {
            { ScratchOnlineCode, SystemSources.IsOnline },
        }.ToFrozenDictionary();
    
    public bool FromSystem => _source.IsT0;
    public bool FromVariable => _source.IsT1;

    public SystemSources SystemSource => _source.AsT0;
    public Variable Variable => _source.AsT1;
    
    public override string ToString() => $"{{{
        _source switch {
            { IsT0: true, AsT0: var t } => "Sys: " + t.Humanize(LetterCasing.Title),
            { IsT1: true, AsT1: var t } => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}}";
    
    public enum SystemSources : byte
    {
        IsOnline
    }

}