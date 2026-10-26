using System.Collections.Frozen;
using Humanizer;
using OneOf;

namespace ScratchCompiler.Parser.Operations;

public class GetFloat : FloatArgument
{
    public const string ScratchXPosCode = "motion_xposition";
    public const string ScratchYPosCode = "motion_yposition";
    public const string ScratchRotationCode = "motion_direction";
    public const string ScratchSizeCode = "looks_size";
    public const string ScratchVolumeCode = "sound_volume";
    
    public static readonly FrozenDictionary<string, SystemSources> SystemSourcesMap = 
        new Dictionary<string, SystemSources>
        {
            { ScratchXPosCode,     SystemSources.XPosition },
            { ScratchYPosCode,     SystemSources.YPosition },
            { ScratchRotationCode, SystemSources.Rotation },
            { ScratchSizeCode,     SystemSources.Size },
            { ScratchVolumeCode,   SystemSources.Volume },
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
            { IsT0: true, AsT0: var t } => "System: " + t.Humanize(LetterCasing.Title),
            { IsT1: true, AsT1: var t } => t.Name,
            _ => throw new InvalidOperationException()
        }
    }}}";
    
    public enum SystemSources : byte
    {
        XPosition,
        YPosition,
        Rotation,
        Volume,
        Size,
    }

}