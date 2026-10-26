using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class Actor: Target
{
    internal Actor(JsonObject json) : base(json) { }
}