using OneOf;
using ScratchCompiler.Decoder.Operations;

namespace ScratchCompiler.Decoder.Functions.Motion;

public class MoveTo : Function
{
    public const string ScratchCodeRelative = "motion_goto";
    public const string ScratchCodeAbsolute = "motion_gotoxy";
    
    public const string ScratchSelectionCode = "motion_goto_menu";
    

    public readonly OneOf<RelativePosition, (FloatArgument, FloatArgument)> Destination;
    
    public MoveTo(RelativePosition target) => Destination = target;
    public MoveTo((FloatArgument x, FloatArgument y) target) => Destination = target;
    public MoveTo(FloatArgument x, FloatArgument y) => Destination = (x, y);

    public override string ToString() => $"Move to { Destination switch {
        {IsT0: true, AsT0: var t0} => $"[{t0} Position]",
        {IsT1: true, AsT1: var t1} => $"[x: {
            (t1.Item1 is IConstArgument and IBracketedString x ? x.ToString(false) : t1.Item1.ToString())}, y: {
            (t1.Item2 is IConstArgument and IBracketedString y ? y.ToString(false) : t1.Item2.ToString())}]",
        _ => throw new NotSupportedException()
    }};";
    
    public enum RelativePosition : byte
    {
        Random, Mouse
    }
}