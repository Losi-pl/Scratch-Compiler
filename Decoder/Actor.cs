using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public class Actor: Target
{
    internal Actor(JsonObject json) : base(json) { }
}