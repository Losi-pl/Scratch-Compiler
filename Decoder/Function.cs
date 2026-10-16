using System.Globalization;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Functions;
using ScratchCompiler.Decoder.Functions.Looks;
using ScratchCompiler.Decoder.Functions.Motion;

namespace ScratchCompiler.Decoder;

public abstract class Function
{

    public static Function LoadFunction(JsonObject json, JsonObject list)
    {
        var opcode = json["opcode"]?.GetValue<string>() ?? string.Empty;
        switch (opcode)
        {
            case MoveBySteps.ScratchCode:
            {
                var steps = json.GetInputFloat("STEPS");
                return new MoveBySteps(steps);
            }
            case Rotate.ScratchCodeRight:
            case Rotate.ScratchCodeLeft:
            {
                var degrees = json.GetInputFloat("DEGREES");
                return new Rotate(degrees, opcode == Rotate.ScratchCodeLeft ? Rotate.TurnDirection.Left : Rotate.TurnDirection.Right);
            }
            case MoveTo.ScratchCodeRelative:
            {
                var @ref = json.GetInputReference("TO", list);
                var mode = @ref.GetField("TO") switch
                {
                    "_mouse_" => MoveTo.RelativePosition.Mouse,
                    "_random_" => MoveTo.RelativePosition.Random,
                    _ => throw new NotSupportedException()
                };
                return new MoveTo(mode);
            }
            case MoveTo.ScratchCodeAbsolute:
            {
                var x = json.GetInputFloat("X");
                var y = json.GetInputFloat("Y");
                return new MoveTo((x, y));
            }
            case GlideTo.ScratchCodeRelative:
            {
                var @ref = json.GetInputReference("TO", list);
                var mode = @ref.GetField("TO") switch
                {
                    "_mouse_" => MoveTo.RelativePosition.Mouse,
                    "_random_" => MoveTo.RelativePosition.Random,
                    _ => throw new NotSupportedException()
                };
                var time = json.GetInputFloat("SECS");
                return new GlideTo(time, mode);
            }
            case GlideTo.ScratchCodeAbsolute:
            {
                var x = json.GetInputFloat("X");
                var y = json.GetInputFloat("Y");
                var time = json.GetInputFloat("SECS");
                return new GlideTo(time, x, y);
            }
            case Say.ScratchCode:
            {
                var text = json.GetInputString("MESSAGE");
                return new Say(text);
            }
            case Think.ScratchCode:
            {
                var text = json.GetInputString("MESSAGE");
                return new Think(text);
            }
            case NextBackground.ScratchCode:
                return new NextBackground();
            default:
                return new UnknownFunction(json);
        }
    }
}

file static class Helper
{
    public static float GetInputFloat(this JsonObject json, string key)
    {
        var str = json["inputs"]?[key]?[1]?[1]?.GetValue<string>();
        return float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : float.NaN;
    }
    public static string GetInputString(this JsonObject json, string key) =>
        json["inputs"]?[key]?[1]?[1]?.GetValue<string>() ?? string.Empty;
    public static string GetField(this JsonObject json, string key) =>
        json["fields"]?[key]?[0]?.GetValue<string>() ?? string.Empty;
    
    public static JsonObject GetInputReference(this JsonObject json, string key, JsonObject list)
    {
        var code = json["inputs"]?[key]?[1]?.GetValue<string>();
        return list[code!]!.AsObject();
    }
}