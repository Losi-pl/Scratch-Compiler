using System.Globalization;
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
            
        }
        if(FloatArgument.Parse(json, list) is { } fArg)
            return fArg;
        return null;
    }
}

public abstract class LogicArgument : Argument
{
    
}

public abstract class FloatArgument : Argument
{
    public static FloatArgument? Parse(JsonObject json, JsonObject list)
    {
        var opcode = json["opcode"]!.GetValue<string>();
        switch (opcode)
        {
            case GetFloat.ScratchXPosCode or GetFloat.ScratchYPosCode or GetFloat.ScratchRotationCode:
                return new GetFloat(GetFloat.SystemSourcesMap[opcode]);
            
        }
        return null;
    }
}
public abstract class IntArgument : FloatArgument
{
    
}