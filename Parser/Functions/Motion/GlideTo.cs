using OneOf;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Parser.Functions.Motion;

public class GlideTo : Function
{
    public const string ScratchCodeRelative = "motion_glideto";
    public const string ScratchCodeAbsolute = "motion_glidesecstoxy";
    
    public const string ScratchSelectionCode = "motion_glideto_menu";
    
    public readonly OneOf<MoveTo.RelativePosition, (FloatArgument, FloatArgument)> Destination;
    public readonly FloatArgument Time;
    
    public GlideTo(FloatArgument time, MoveTo.RelativePosition target) => (Destination, Time) = (target, time);
    public GlideTo(FloatArgument time, (FloatArgument x, FloatArgument y) target) => (Destination, Time) = (target, time);
    public GlideTo(FloatArgument time, FloatArgument x, FloatArgument y) => (Destination, Time) = ((x, y), time);

    public override string ToString() => $"Glide for {Time} secs to { Destination switch {
        {IsT0: true, AsT0: var t0} => $"[{t0} Position]",
        {IsT1: true, AsT1: var t1} => $"[x: {
            (t1.Item1 is IConstArgument and IBracketedString x ? x.ToString(false) : t1.Item1.ToString())}, y: {
                (t1.Item2 is IConstArgument and IBracketedString y ? y.ToString(false) : t1.Item2.ToString())}]",
        _ => throw new NotSupportedException()
    }};";
}