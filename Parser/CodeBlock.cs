using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Nodes;
using ScratchCompiler.Parser.Entries;

namespace ScratchCompiler.Parser;

public class CodeBlock
{
    private readonly CodeEntry _entry;
    private readonly ImmutableArray<Function> _functions;

    internal CodeBlock(JsonObject json, JsonObject entry, Target target)
    {
        _entry = CodeEntry.LoadEntry(entry, json);
        List<Function> functions = [];
        
        var current = entry;
        if (_entry is not Dormant)
            current = Next(current);

        while (current is not null)
        {
            functions.Add(Function.LoadFunction(current, json, target));
            current = Next(current);
        }
        
        // ReSharper disable once UseCollectionExpression
        _functions = functions.ToImmutableArray();
        
        // ReSharper disable once VariableHidesOuterVariable
        JsonObject? Next(JsonObject current)
        {
            var nextCode = current["next"]?.GetValue<string>();
            return nextCode is null ? null : json[nextCode]!.AsObject();
        }
    }

    public string ToScript()
    {
        StringBuilder sb = new();
        sb.Append(_entry);
        foreach (var function in _functions)
        {
            sb.Append("\n    ");
            sb.AppendJoin("\n    ", function.ToString()!.Split('\n'));
        }
        
        return  sb.ToString();
    }
}