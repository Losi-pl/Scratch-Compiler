using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public class CodeBlock
{
    private readonly CodeEntry _entry;
    private Function[] _functions;

    internal CodeBlock(JsonObject json, JsonObject entry)
    {
        var meCodeName = entry.GetPropertyName();
        _entry = CodeEntry.LoadEntry(entry);
        
        var current = entry;
        if (_entry is Dormant)
            current = Next(current);

        // ReSharper disable once VariableHidesOuterVariable
        JsonObject? Next(JsonObject current)
        {
            var nextCode = current["next"]?.GetValue<string>();
            return nextCode is null ? null : json[nextCode]!.AsObject();
        }
    }
}