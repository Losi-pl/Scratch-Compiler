using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public class Scene: Target
{
    internal Scene(JsonObject json) : base(json) { }
}