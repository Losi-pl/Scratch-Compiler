using System.Globalization;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Functions;
using ScratchCompiler.Decoder.Functions.Looks;
using ScratchCompiler.Decoder.Functions.Motion;
using ScratchCompiler.Decoder.Functions.Data;

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
            case Rotate.ScratchCodeRight or Rotate.ScratchCodeLeft:
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
            case PointTowards.ScratchCode:
            {
                var @ref = json.GetInputReference("TOWARDS", list);
                var mode = @ref.GetField("TOWARDS");
                if (mode != "_mouse_")
                    throw new InvalidCastException();
                return new PointTowards();
            }
            case SetVariable.ScratchSetXCode or SetVariable.ScratchSetYCode or SetVariable.ScratchRotationCode:
            {
                var value = json.GetInputFloat(SetVariable.SystemVariables[opcode].Value);
                return new SetVariable(SetVariable.SystemVariables[opcode].Type, value);
            }
            case SetVariable.ScratchCode:
            {
                var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
                var variable = Project.CurrentProject.project.Variables.FindByScratchCode(@ref);
                return new SetVariable(variable ?? throw new KeyNotFoundException(), json.GetInputFloat("VALUE"));
            }
            case ChangeVariable.ScratchXCode or ChangeVariable.ScratchYCode:
            {
                var value = json.GetInputFloat(ChangeVariable.SystemVariables[opcode].Value);
                return new ChangeVariable(ChangeVariable.SystemVariables[opcode].Type, value);
            }
            case ChangeVariable.ScratchCode:
            {
                var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
                var variable = Project.CurrentProject.project.Variables.FindByScratchCode(@ref);
                return new ChangeVariable(variable ?? throw new KeyNotFoundException(), json.GetInputFloat("VALUE"));
            }
            case Say.ScratchCode or Think.ScratchCode:
            {
                var text = json.GetInputString("MESSAGE");
                return opcode == Say.ScratchCode ? new Say(text) : new Think(text);
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