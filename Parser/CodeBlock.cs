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
        if (_entry is Dormant)
        {
            functions.Add(Function.LoadFunction(entry, json, target));
        }
        
        var next = entry["next"]?.GetValue<string>();
        if(!string.IsNullOrEmpty(next))
            functions.AddRange(GetMethodString(json, next, target));

        // ReSharper disable once UseCollectionExpression
        _functions = functions.ToImmutableArray();
        
        // ReSharper disable once VariableHidesOuterVariable
        JsonObject? Next(JsonObject current)
        {
            var nextCode = current["next"]?.GetValue<string>();
            return nextCode is null ? null : json[nextCode]!.AsObject();
        }
    }

    public CodeEntry Entry => _entry;
    public ImmutableArray<Function> Methods => _functions;
    
    public string ToScript()
    {
        StringBuilder sb = new();
        sb.Append(_entry);
        var isEmpty = true;
        foreach (var function in _functions)
        {
            isEmpty = false;
            sb.Append("\n    ");
            sb.AppendJoin("\n    ", function.ToString()!.Split('\n'));
        }
        
        sb.Append($"{(isEmpty ? "\n\n" : "\n")}End;");
        
        return  sb.ToString();
    }

    public static IEnumerable<Function> GetMethodString(JsonObject list, string start, Target target)
    {
        var current = list[start]?.AsObject() ?? throw new NullReferenceException();
        yield return Function.LoadFunction(current, list, target);

        var nextCode = current["next"]?.GetValue<string>();
        while (nextCode is not null)
        {
            current = list[nextCode]!.AsObject();
            nextCode = current["next"]?.GetValue<string>();
            yield return Function.LoadFunction(current, list, target);
        }
    }
}