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

    public static Function LoadFunction(JsonObject json, JsonObject list, Target target)
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
            case SetVariable.ScratchSetXCode or SetVariable.ScratchSetYCode or 
                SetVariable.ScratchRotationCode or SetVariable.ScratchSizeCode or 
                SetVariable.ScratchVolumeCode:
            {
                var value = json.GetGeneralInput(list, SetVariable.SystemVariables[opcode].Value);
                return new SetVariable(SetVariable.SystemVariables[opcode].Type, value);
            }
            case SetVariable.ScratchCode:
            {
                var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
                var variable = Project.Current.project.Variables.FindByScratchCode(@ref);
                return new SetVariable(variable ?? throw new KeyNotFoundException(), json.GetGeneralInput(list, "VALUE"));
            }
            case ChangeVariable.ScratchXCode or ChangeVariable.ScratchYCode or 
                ChangeVariable.ScratchSizeCode or ChangeVariable.ScratchVolumeCode:
            {
                var value = json.GetGeneralInput(list, ChangeVariable.SystemVariables[opcode].Value);
                return new ChangeVariable(ChangeVariable.SystemVariables[opcode].Type, value);
            }
            case ChangeVariable.ScratchCode:
            {
                var @ref = json["fields"]!["VARIABLE"]![1]!.GetValue<string>();
                var variable = Project.Current.project.Variables.FindByScratchCode(@ref);
                return new ChangeVariable(variable ?? throw new KeyNotFoundException(), json.GetGeneralInput(list, "VALUE"));
            }
            case Say.ScratchCode or Think.ScratchCode:
            {
                var text = json.GetStringInput(list, "MESSAGE");
                return opcode == Say.ScratchCode ? new Say(text) : new Think(text);
            }
            case SwitchBackground.ScratchNextCode:
                return new SwitchBackground(SwitchBackground.Relative.Next);
            case SwitchBackground.ScratchCode:
            {
                var @ref = json.GetInputReference("BACKDROP", list);
                var dest = @ref.GetField("BACKDROP");
                var background = Project.Current.project.Scene.Costumes.FirstOrDefault(b => b.Name == dest);
                if (background is not null)
                    return new SwitchBackground(background);
                return new SwitchBackground(dest switch
                {
                    "next backdrop" => SwitchBackground.Relative.Next,
                    "previous backdrop" => SwitchBackground.Relative.Previous,
                    "random backdrop" => SwitchBackground.Relative.Random,
                    _ => throw new NotSupportedException($"Unknown background {{{dest}}}")
                });
            }
            case SwitchCostume.ScratchCode:
            {
                var @ref = json.GetInputReference("COSTUME", list);
                var dest = @ref.GetField("COSTUME");
                var costume = target.Costumes.FirstOrDefault(c => c.Name == dest) ?? throw new KeyNotFoundException(dest);
                return new SwitchCostume(costume);
            }
            case ChangeVariable.ScratchEffectsCode:
            {
                var effect = json.GetField("EFFECT");
                var change = json.GetGeneralInput(list, "CHANGE");
                return new ChangeVariable(SetVariable.GraphicEffects[effect], change);
            }
            case SetVariable.ScratchEffectCode:
            {
                var effect = json.GetField("EFFECT");
                var value = json.GetGeneralInput(list, "VALUE");
                return new SetVariable(SetVariable.GraphicEffects[effect], value);
            }
            case SetRotationStyle.ScratchCode:
            {
                var style = json.GetField("STYLE");
                return new SetRotationStyle(SetRotationStyle.ModesMap[style]);
            }
            case SayFor.ScratchCode or ThinkFor.ScratchCode:
            {
                var text = json.GetGeneralInput(list, "MESSAGE");
                var time = json.GetFloatInput(list, "SECS");
                return opcode == SayFor.ScratchCode ? new SayFor(text, time) : new ThinkFor(text, time);
            }
            case MoveLayers.ScratchCode:
            {
                var direction = json.GetField("FORWARD_BACKWARD");
                var move = json.GetIntInput(list, "NUM");
                return new MoveLayers(MoveLayers.DirectionMap[direction], move);
            }
            case MoveToLayer.ScratchCode:
            {
                var destination = json.GetField("FRONT_BACK");
                return new MoveToLayer(MoveToLayer.DestinationMap[destination]);
            }
            case ShowHide.ScratchShowCode or ShowHide.ScratchHideCode:
                return new ShowHide(opcode == ShowHide.ScratchShowCode);
            case SwitchCostume.ScratchNextCode:
                return new SwitchCostume(SwitchCostume.RelativeMode.Next);
            case BounceFromEdge.ScratchCode:
                return BounceFromEdge.Instance;
            case ClearEffects.ScratchCode:
                return ClearEffects.Instance;
            default:
                return new UnknownFunction(json);
        }
    }
}

internal static class ArgHelper
{
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
    public static IntArgument GetIntInput(this JsonObject json, JsonObject list, string key)
    {
        switch (json["inputs"]?[key]?[0]!.GetValue<int>())
        {
            case 1:
            {
                var str = json["inputs"]?[key]?[1]?[1]?.GetValue<string>();
                var @int = int.TryParse(str, out var i) ? i : 0;
                return new ConstInt(@int);
            }
                
            case 3:
            {
                if (json["inputs"]?[key]?[1]?.GetValueKind() == JsonValueKind.String)
                {
                    var @ref = json["inputs"]?[key]?[1]?.GetValue<string>() ?? string.Empty;
                    var arg = list[@ref]!.AsObject();
                    return IntArgument.Parse(arg, list) ?? 
                           new UnknownIntArgument(
                               arg["opcode"]?.GetValue<string>() ?? string.Empty,
                               int.TryParse(json["inputs"]?[key]?[2]?[1]?.GetValue<string>() ?? string.Empty, 
                                   NumberStyles.Float, CultureInfo.InvariantCulture, out var i) ? i : 0);
                }
                if (json["inputs"]?[key]?[1]?[0]?.GetValue<int>() == 12)
                {
                    var @ref = json["inputs"]?[key]?[1]?[2]?.GetValue<string>();
                    var var = Project.Current.project.Variables.FindByScratchCode(@ref!);
                    return new GetInt(var ?? throw new KeyNotFoundException());
                }
                
                throw new KeyNotFoundException();
            }
            default:
                throw new NotSupportedException("Unknown input type");
        }
    }
    public static Argument GetGeneralInput(this JsonObject json, JsonObject list, string key)
    {
        switch (json["inputs"]?[key]?[0]!.GetValue<int>())
        {
            case 1:
            {
                var str = json["inputs"]?[key]?[1]?[1]?.GetValue<string>();
                if(int.TryParse(str, out var i))
                    return new ConstInt(i);
                if(float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    return new ConstFloat(f);
                return str switch
                {
                    "true" => new ConstBool(true),
                    "false" => new ConstBool(false),
                    _ => new ConstString(str ?? string.Empty)
                };
            }
            case 3:
            {
                var arg = json["inputs"]?[key]?[1];
                switch (arg?.GetValueKind())
                {
                    case JsonValueKind.String:
                    {
                        var @ref = arg.GetValue<string>();
                        var argument = Argument.Parse(list[@ref]!.AsObject(), list);
                        if (argument is not null)
                            return argument;
                        else
                        {
                            var opcode = list[@ref]!["opcode"]?.GetValue<string>() ?? string.Empty;
                            var @const = json["inputs"]?[key]?[2]?[1]?.GetValue<string>() ?? string.Empty;
                            if (int.TryParse(@const, out var i))
                                return new UnknownGeneralArgument(opcode, i);
                            if (float.TryParse(@const, out var f))
                                return new UnknownGeneralArgument(opcode, f);
                            if (bool.TryParse(@const, out var b))
                                return new UnknownGeneralArgument(opcode, b);
                            return new UnknownGeneralArgument(opcode, @const);
                        }
                    }
                    case JsonValueKind.Array:
                    {
                        var varCode = json["inputs"]?[key]?[1]?[2]?.GetValue<string>() ?? string.Empty;
                        var variable = Project.Current.project.Variables.FindByScratchCode(varCode);
                        if(variable is not null)
                            return new GeneralGet(variable);
                        else
                        {
                            var @const = json["inputs"]?[key]?[2]?[1]?.GetValue<string>() ?? string.Empty;
                            if (int.TryParse(@const, out var i))
                                return new ConstInt(i);
                            if (float.TryParse(@const, out var f))
                                return new ConstFloat(f);
                            if (bool.TryParse(@const, out var b))
                                return new ConstBool(b);
                            return new ConstString(@const);
                        }
                    }
                }
            } break;
        }
        return null!;
    }
    public static JsonObject GetInputReference(this JsonObject json, string key, JsonObject list)
    {
        var code = json["inputs"]?[key]?[1]?.GetValue<string>();
        return list[code!]!.AsObject();
    }

    public static string ToSystemMarker(this SetVariable.SysVariables variable) => variable switch
    {
        SetVariable.SysVariables.Color or SetVariable.SysVariables.Fisheye or 
            SetVariable.SysVariables.Whirl or SetVariable.SysVariables.Pixelate or 
            SetVariable.SysVariables.Mosaic or SetVariable.SysVariables.Brightness or 
            SetVariable.SysVariables.Ghost => "Graphic",
        _ => "System"
    };
}