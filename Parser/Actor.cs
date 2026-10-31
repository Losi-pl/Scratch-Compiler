using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class Actor: Target
{
    internal Actor(JsonObject json) : base(json)
    {
        {
            var x = json["x"]?.GetValue<float>() ?? float.NaN;
            var y = json["y"]?.GetValue<float>() ?? float.NaN;
            DefaultPosition = (x, y);
        }
    }
    
    public (float x, float y) DefaultPosition { get; }
}