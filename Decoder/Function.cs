using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Functions;
using ScratchCompiler.Decoder.Functions.Looks;
using ScratchCompiler.Decoder.Functions.Motion;
using ScratchCompiler.Decoder.Functions.Data;
using ScratchCompiler.Decoder.Operations;

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
                var steps = json.GetFloatInput(list, "STEPS");
                return new MoveBySteps(steps);
            }
            case Rotate.ScratchCodeRight or Rotate.ScratchCodeLeft:
            {
                var degrees = json.GetFloatInput(list, "DEGREES");
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
                var x = json.GetFloatInput(list, "X");
                var y = json.GetFloatInput(list, "Y");
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
                var time = json.GetFloatInput(list, "SECS");
                return new GlideTo(time, mode);
            }
            case GlideTo.ScratchCodeAbsolute:
            {
                var x = json.GetFloatInput(list, "X");
                var y = json.GetFloatInput(list, "Y");
                var time = json.GetFloatInput(list, "SECS");
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
                var variable = Project.Current.project.Variables.FindByScratchCode(@ref);
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
                var variable = Project.Current.project.Variables.FindByScratchCode(@ref);
                return new ChangeVariable(variable ?? throw new KeyNotFoundException(), json.GetInputFloat("VALUE"));
            }
            case Say.ScratchCode or Think.ScratchCode:
            {
                var text = json.GetStringInput(list, "MESSAGE");
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

    public static Argument GetStringInput(this JsonObject json, JsonObject list, string key)
    {
        switch (json["inputs"]?[key]?[0]!.GetValue<int>())
        {
            case 1:
                return new ConstString(json["inputs"]?[key]?[1]?[1]?.GetValue<string>() ?? string.Empty);
            case 3:
            {
                if (json["inputs"]?[key]?[1]?.GetValueKind() == JsonValueKind.String)
                {
                    var @ref = json["inputs"]?[key]?[1]?.GetValue<string>() ?? string.Empty;
                    var arg = list[@ref]!.AsObject();
                    return Argument.Parse(arg, list) ?? 
                           new UnknownStringArgument(
                               arg["opcode"]?.GetValue<string>() ?? string.Empty,
                               json["inputs"]?[key]?[2]?[1]?.GetValue<string>() ?? string.Empty);
                }
                if (json["inputs"]?[key]?[1]?[0]?.GetValue<int>() == 12)
                {
                    var @ref = json["inputs"]?[key]?[1]?[2]?.GetValue<string>();
                    var var = Project.Current.project.Variables.FindByScratchCode(@ref!);
                    return new GetString(var ?? throw new KeyNotFoundException());
                }
                
                throw new KeyNotFoundException();
            }
            default:
                throw new NotSupportedException("Unknown input type");
        }
    }

    public static FloatArgument GetFloatInput(this JsonObject json, JsonObject list, string key)
    {
        switch (json["inputs"]?[key]?[0]!.GetValue<int>())
        {
            case 1:
            {
                var str = json["inputs"]?[key]?[1]?[1]?.GetValue<string>();
                var @float = float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : float.NaN;
                return new ConstFloat(@float);
            }
                
            case 3:
            {
                if (json["inputs"]?[key]?[1]?.GetValueKind() == JsonValueKind.String)
                {
                    var @ref = json["inputs"]?[key]?[1]?.GetValue<string>() ?? string.Empty;
                    var arg = list[@ref]!.AsObject();
                    return FloatArgument.Parse(arg, list) ?? 
                           new UnknownFloatArgument(
                               arg["opcode"]?.GetValue<string>() ?? string.Empty,
                               float.TryParse(json["inputs"]?[key]?[2]?[1]?.GetValue<string>() ?? string.Empty, 
                                   NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : float.NaN);
                }
                if (json["inputs"]?[key]?[1]?[0]?.GetValue<int>() == 12)
                {
                    var @ref = json["inputs"]?[key]?[1]?[2]?.GetValue<string>();
                    var var = Project.Current.project.Variables.FindByScratchCode(@ref!);
                    return new GetFloat(var ?? throw new KeyNotFoundException());
                }
                
                throw new KeyNotFoundException();
            }
            default:
                throw new NotSupportedException("Unknown input type");
        }
    }
    
    public static JsonObject GetInputReference(this JsonObject json, string key, JsonObject list)
    {
        var code = json["inputs"]?[key]?[1]?.GetValue<string>();
        return list[code!]!.AsObject();
    }
}