using System.Collections.Frozen;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using ScratchCompiler.Parser.Functions;
using ScratchCompiler.Parser.Functions.Control;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Functions.Motion;
using ScratchCompiler.Parser.Functions.Data;
using ScratchCompiler.Parser.Functions.Events;
using ScratchCompiler.Parser.Functions.Sensing;
using ScratchCompiler.Parser.Functions.Sound;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Parser;

public abstract class Function
{
    private static FrozenDictionary<string, Func<JsonObject, JsonObject, Target, string, Function>> FunctionParsers = typeof(Function).Assembly.GetTypes()
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        .Where(m => m.GetCustomAttributes<FunctionParserAttribute>().Any())
        .Where(m => {
            if (!(m.ReturnType == typeof(Function) || m.ReturnType.IsSubclassOf(typeof(Function))))
                return false;
            
            var args = m.GetParameters();
            if (args.Length is not (3 or 4))
                return false;
            
            if (args[0].ParameterType != typeof(JsonObject))
                return false;
            if (args[1].ParameterType != typeof(JsonObject))
                return false;
            if (args[2].ParameterType != typeof(Target))
                return false;
            if (args.Length is 4 && args[3].ParameterType != typeof(string))
                return false;

            return true;
        }).SelectMany(m =>
        {
            Func<JsonObject, JsonObject, Target, string, Function> invoke;
            if (m.GetParameters().Length == 4)
                invoke = (me, list, target, myOpcode) => (Function)m.Invoke(null, [me, list, target, myOpcode])!;
            else
                invoke = (me, list, target, _) => (Function)m.Invoke(null, [me, list, target])!;

            return m.GetCustomAttributes<FunctionParserAttribute>().Select(op =>
                new KeyValuePair<string, Func<JsonObject, JsonObject, Target, string, Function>>(op.OpCode, invoke));
        }).ToFrozenDictionary();

    public static Function LoadFunction(JsonObject json, JsonObject list, Target target)
    {
        var opcode = json["opcode"]?.GetValue<string>() ?? string.Empty;
        if(FunctionParsers.TryGetValue(opcode, out var pars))
            if (pars(json, list, target, opcode) is { } function)
                return function;
        
        return new UnknownFunction(json);
    }
}

internal static class BlockHelpers
{
    public static string GetField(this JsonObject json, string key) =>
        json["fields"]?[key]?[0]?.GetValue<string>() ?? string.Empty;
    
    public static Color GetColor(this JsonObject json, string key)
    {
        var code = (json["inputs"]?[key]?[1]?[1]?.GetValue<string>() ?? throw new InvalidCastException()).AsSpan();
        var r = int.Parse(code[1..3], NumberStyles.HexNumber);
        var g = int.Parse(code[3..5], NumberStyles.HexNumber);
        var b = int.Parse(code[5..7], NumberStyles.HexNumber);
        return Color.FromArgb(r, g, b);
    }

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
                    var var = SyntaxTree.Current.project.Variables.FindByScratchCode(@ref!);
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
                    var var = SyntaxTree.Current.project.Variables.FindByScratchCode(@ref!);
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
                    var var = SyntaxTree.Current.project.Variables.FindByScratchCode(@ref!);
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
                        var variable = SyntaxTree.Current.project.Variables.FindByScratchCode(varCode);
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
    public static LogicArgument GetLogicInput(this JsonObject json, JsonObject list, string key)
    {
        switch (json["inputs"]?[key]?[0]!.GetValue<int>())
        {
            case 1:
                return new NoLogic();
            case 2:
            {
                var @ref = json["inputs"]?[key]?[1]?.GetValue<string>() ?? throw new KeyNotFoundException("No key found");
                var argument = LogicArgument.Parse(list[@ref]!.AsObject(), list);
                if (argument is null)
                    return new UnknownLogicArgument(list[@ref]?["opcode"]?.GetValue<string>() ?? string.Empty);
                return argument;
            }
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
        SetVariable.SysVariables.Pitch or SetVariable.SysVariables.PanStereo => "Sound",
        _ => "System"
    };
}