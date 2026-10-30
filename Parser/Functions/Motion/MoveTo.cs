using System.Text.Json.Nodes;
using OneOf;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Parser.Functions.Motion;

public class MoveTo : Function
{
    public const string ScratchRelativeCode = "motion_goto";
    public const string ScratchAbsoluteCode = "motion_gotoxy";
    
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
    
    [FunctionParser(ScratchRelativeCode)] // ReSharper disable once UnusedMember.Local
    private static MoveTo ParseRelative(JsonObject json, JsonObject list, Target _) {
        var @ref = json.GetInputReference("TO", list);
        var mode = @ref.GetField("TO") switch
        {
            "_mouse_" => RelativePosition.Mouse,
            "_random_" => RelativePosition.Random,
            _ => throw new NotSupportedException()
        };
        return new MoveTo(mode);
    }
    
    [FunctionParser(ScratchAbsoluteCode)] // ReSharper disable once UnusedMember.Local
    private static MoveTo ParseAbsolute(JsonObject json, JsonObject list, Target _) {
        var x = json.GetFloatInput(list, "X");
        var y = json.GetFloatInput(list, "Y");
        return new MoveTo((x, y));
    }
    
    public enum RelativePosition : byte
    {
        Random, Mouse
    }
}