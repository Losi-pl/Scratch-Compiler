using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class Actor: Target
{
    internal Actor(JsonObject json) : base(json)
    {
        {
            var x = json["x"]?.GetValue<double>() ?? double.NaN;
            var y = json["y"]?.GetValue<double>() ?? double.NaN;
            DefaultPosition = (x, y);
        }
    }
    
    public (double x, double y) DefaultPosition { get; }
}