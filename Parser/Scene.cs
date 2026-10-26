using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class Scene: Target
{
    internal Scene(JsonObject json) : base(json) { }
}