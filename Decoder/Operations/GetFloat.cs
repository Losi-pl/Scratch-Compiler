using System.Collections.Frozen;
using Humanizer;
using OneOf;

namespace ScratchCompiler.Decoder.Operations;

public class GetFloat : FloatArgument
{
    public const string ScratchXPosCode = "motion_xposition";
    public const string ScratchYPosCode = "motion_yposition";
    public const string ScratchRotationCode = "motion_direction";
    public const string ScratchSizeCode = "looks_size";
    
    public static readonly FrozenDictionary<string, SystemSources> SystemSourcesMap = 
        new Dictionary<string, SystemSources>
        {
            { ScratchXPosCode, SystemSources.XPosition },
            { ScratchYPosCode, SystemSources.YPosition },
            { ScratchRotationCode, SystemSources.Rotation },
            { ScratchSizeCode, SystemSources.Size },
        }.ToFrozenDictionary();
    
    private readonly OneOf<SystemSources, Variable> _source;
    
    public GetFloat(SystemSources source) => _source = source;
    public GetFloat(Variable source) => _source = source;

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
        XPosition,
        YPosition,
        Rotation,
        Size
    }

}