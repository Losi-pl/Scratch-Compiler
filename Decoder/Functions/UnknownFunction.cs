using System.Collections.Frozen;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder.Functions;

public class UnknownFunction : Function
{
    private readonly FrozenDictionary<string, JsonNode> _entries;

    public UnknownFunction(JsonObject block)
    {
        _entries = block.Where(kvp => kvp.Key is not ("parent" or "next" or "shadow" or "topLevel" or "x" or "y")).ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value?.DeepClone()!);
    }

    public override string ToString() => $"Code {{{_entries["opcode"]}}};";
}