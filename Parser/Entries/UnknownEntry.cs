using System.Collections.Frozen;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Entries;

public class UnknownEntry: CodeEntry
{
    private readonly FrozenDictionary<string, JsonNode> _entries;

    public UnknownEntry(JsonObject block)
    {
        _entries = block.Where(kvp => kvp.Key is not ("parent" or "next" or "shadow" or "topLevel" or "x" or "y")).ToFrozenDictionary(kvp => kvp.Key, kvp => kvp.Value?.DeepClone()!);
    }

    public override string ToString() => $"Unknown event {{{_entries["opcode"]}}}:";
}