using System.Collections.Frozen;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder.Entries;

public class UnknownEntry: CodeEntry
{
    private readonly FrozenDictionary<string, JsonNode> _entries;

    public UnknownEntry(JsonObject block)
    {
        _entries = block.ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value?.DeepClone()!);
    }
}