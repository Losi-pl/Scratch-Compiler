using System.Text.Json.Nodes;
using OneOf;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Parser.Functions.Motion;

public class GlideTo : Function
{
    public const string ScratchRelativeCode = "motion_glideto";
    public const string ScratchAbsoluteCode = "motion_glidesecstoxy";
    
    public const string ScratchSelectionCode = "motion_glideto_menu";
    
    public readonly OneOf<MoveTo.RelativePosition, (FloatArgument, FloatArgument)> Destination;
    public readonly FloatArgument Time;
    
    private GlideTo(FloatArgument time, MoveTo.RelativePosition target) => (Destination, Time) = (target, time);
    private GlideTo(FloatArgument time, (FloatArgument x, FloatArgument y) target) => (Destination, Time) = (target, time);
    private GlideTo(FloatArgument time, FloatArgument x, FloatArgument y) => (Destination, Time) = ((x, y), time);

    public override string ToString() => $"Glide for {Time} secs to { Destination switch {
        {IsT0: true, AsT0: var t0} => $"[{t0} Position]",
        {IsT1: true, AsT1: var t1} => $"[x: {
            (t1.Item1 is IConstArgument and IBracketedString x ? x.ToString(false) : t1.Item1.ToString())}, y: {
                (t1.Item2 is IConstArgument and IBracketedString y ? y.ToString(false) : t1.Item2.ToString())}]",
        _ => throw new NotSupportedException()
    }};";
    
    [FunctionParser(ScratchRelativeCode)] // ReSharper disable once UnusedMember.Local
    private static GlideTo ParseRelative(JsonObject json, JsonObject list, Target _) {
        var @ref = json.GetInputReference("TO", list);
        var mode = @ref.GetField("TO") switch
        {
            "_mouse_" => MoveTo.RelativePosition.Mouse,
            "_random_" => MoveTo.RelativePosition.Random,
            _ => throw new NotSupportedException()
        };
        var time = json.GetFloatInput(list, "SECS");
        return new GlideTo(time, mode);
    }
    
    [FunctionParser(ScratchAbsoluteCode)] // ReSharper disable once UnusedMember.Local
    private static GlideTo ParseAbsolute(JsonObject json, JsonObject list, Target _) {
        var x = json.GetFloatInput(list, "X");
        var y = json.GetFloatInput(list, "Y");
        var time = json.GetFloatInput(list, "SECS");
        return new GlideTo(time, x, y);
    }
}