using Humanizer;
using OneOf;

namespace ScratchCompiler.Decoder.Operations;

public class UnknownGeneralArgument(string opcode, OneOf<string, bool, int, float> @const) : Argument
{
    public bool IsString => @const.IsT0;
    public bool IsBool => @const.IsT1;
    public bool IsInt => @const.IsT2;
    public bool IsFloat => @const.IsT3;

    public string AsString => @const.AsT0;
    public bool AsBool => @const.AsT1;
    public int AsInt => @const.AsT2;
    public float AsFloat => @const.AsT3;
    
    public override string ToString() => @const switch {
        {IsT0: true, AsT0: var t } => $"\"{t}\"",
        {IsT1: true, AsT1: var t } => $"[{t}]",
        {IsT2: true, AsT2: var t } => $"[{t}]",
        {IsT3: true, AsT3: var t } => $"[{t}]",
        _ => throw new NotSupportedException()
    };
}