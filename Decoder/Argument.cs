using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Operations;

namespace ScratchCompiler.Decoder;

public abstract class Argument
{
    public static Argument? Parse(JsonObject json, JsonObject list)
    {
        var opcode = json["opcode"]!.GetValue<string>();
        switch (opcode)
        {
            case GetString.ScratchCode:
                return new GetString(GetString.SystemSources.Username);
            case CostumeData.ScratchCostumeCode:
            {
                var source = json.GetField("NUMBER_NAME");
                return source switch
                {
                    "number" => new GetInt(GetInt.SystemSources.CostumeNumber),
                    "name" => new GetString(GetString.SystemSources.CostumeName),

                    _ => throw new NotSupportedException()
                };
            }
            case CostumeData.ScratchBackGroundCode:
            {
                var source = json.GetField("NUMBER_NAME");
                return source switch
                {
                    "number" => new GetInt(GetInt.SystemSources.BackgroundNumber),
                    "name" => new GetString(GetString.SystemSources.BackgroundName),

                    _ => throw new NotSupportedException()
                };
            }
        }
        if(LogicArgument.Parse(json, list) is { } bArg)
            return bArg;
        if(FloatArgument.Parse(json, list) is { } fArg)
            return fArg;
        return null;
    }
}

public abstract class LogicArgument : Argument
{
    public new static LogicArgument? Parse(JsonObject json, JsonObject list)
    {
        var opcode = json["opcode"]!.GetValue<string>();
        switch (opcode)
        {
            case GetBool.ScratchOnlineCode:
                return new GetBool(GetBool.SystemSourcesMap[opcode]);
        }

        return null;
    }
}

public abstract class FloatArgument : Argument
{
    public new static FloatArgument? Parse(JsonObject json, JsonObject list)
    {
        var opcode = json["opcode"]!.GetValue<string>();
        switch (opcode)
        {
            case GetFloat.ScratchXPosCode or GetFloat.ScratchYPosCode or GetFloat.ScratchRotationCode:
                return new GetFloat(GetFloat.SystemSourcesMap[opcode]);
            case AddOperator.ScratchCode:
            {
                var a = json.GetFloatInput(list, "NUM1");
                var b = json.GetFloatInput(list, "NUM2");
                return new AddOperator(a, b);
            }
            case SubtractOperator.ScratchCode:
            {
                var a = json.GetFloatInput(list, "NUM1");
                var b = json.GetFloatInput(list, "NUM2");
                return new SubtractOperator(a, b);
            }
            case MultiplyOperator.ScratchCode:
            {
                var a = json.GetFloatInput(list, "NUM1");
                var b = json.GetFloatInput(list, "NUM2");
                return new MultiplyOperator(a, b);
            }
            case DivideOperator.ScratchCode:
            {
                var a = json.GetFloatInput(list, "NUM1");
                var b = json.GetFloatInput(list, "NUM2");
                return new DivideOperator(a, b);
            }
            case RandomNumber.ScratchCode:
            {
                var from = json.GetFloatInput(list, "FROM");
                var to = json.GetFloatInput(list, "TO");
                return new RandomNumber(from, to);
            }
            case Round.ScratchCode:
            {
                var val = json.GetFloatInput(list, "NUM");
                return new Round(val);
            }
            case Modulo.ScratchName:
            {
                var dividend = json.GetFloatInput(list, "NUM1");
                var divisor = json.GetFloatInput(list, "NUM2");
                return new Modulo(dividend, divisor);
            }
            case MathFunc.ScratchCode:
            {
                var @operator = json.GetField("OPERATOR");
                var value = json.GetFloatInput(list, "NUM");
                return new MathFunc(MathFunc.OperationMap[@operator], value);
            }
        }
        return null;
    }
}
public abstract class IntArgument : FloatArgument
{
    
}