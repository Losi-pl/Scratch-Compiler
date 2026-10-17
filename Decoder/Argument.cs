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
        return null;
    }
}

public abstract class LogicArgument : Argument
{
    
}

public abstract class FloatArgument : Argument
{
    
}
public abstract class IntArgument : FloatArgument
{
    
}