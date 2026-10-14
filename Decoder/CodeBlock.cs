using System.Collections.Immutable;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public class CodeBlock
{
    private readonly CodeEntry _entry;
    private ImmutableArray<Function> _functions;

    internal CodeBlock(JsonObject json, JsonObject entry)
    {
        var meCodeName = entry.GetPropertyName();
        _entry = CodeEntry.LoadEntry(entry);
        List<Function> functions = [];
        
        var current = entry;
        if (_entry is not Dormant)
            current = Next(current);

        while (current is not null)
        {
            functions.Add(Function.LoadFunction(current, json));
            current = Next(current);
        }
        
        _functions = functions.ToImmutableArray();
        
        // ReSharper disable once VariableHidesOuterVariable
        JsonObject? Next(JsonObject current)
        {
            var nextCode = current["next"]?.GetValue<string>();
            return nextCode is null ? null : json[nextCode]!.AsObject();
        }
    }
}