using Humanizer;
using OneOf;

namespace ScratchCompiler.Decoder.Operations;

public class GetString : Argument
{
    public const string ScratchCode = "sensing_username";
    
    private OneOf<SystemSources, Variable> _source;
    
    public GetString(SystemSources source) => _source = source;
    public GetString(Variable source) => _source = source;

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
        Username
    }

}