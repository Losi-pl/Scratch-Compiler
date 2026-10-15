using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public abstract class Target
{
    private JsonObject _jsonCode;
    private FrozenSet<CodeBlock> _blocks = null!;
    private FrozenDictionary<string, Broadcast> _broadcasts;

    internal Target(JsonObject json)
    {
        _jsonCode = json;
        Name = json["name"]!.GetValue<string>();
        DefaultCostume = (uint)(json["currentCostume"]?.GetValue<int>() ?? 0);
        Costumes = GetCostumes(json["costumes"]!.AsArray());
        _broadcasts = json["broadcasts"]!.AsObject().ToFrozenDictionary(kvp => kvp.Key,
            kvp => new Broadcast(kvp.Value?.GetValue<string>() ?? string.Empty));
    }

    public string Name { get; }
    public FrozenSet<CodeBlock> Blocks => _blocks;
    public ImmutableArray<Costume> Costumes { get; }
    public readonly uint DefaultCostume;
    public ImmutableArray<Broadcast> Broadcasts => _broadcasts.Values;

    private ImmutableArray<Costume> GetCostumes(JsonArray json)
    {
        List<Costume> costumes = new();

        foreach (var costume in json.Cast<JsonObject>())
            costumes.Add(new(costume));
        
        // ReSharper disable once UseCollectionExpression
        return costumes.ToImmutableArray();
    }

    internal Broadcast? GetBroadcastByCode(string code) => (_broadcasts?.TryGetValue(code, out var b) ?? false) ? b : null;
    internal void LoadCode()
    {
        var blocks = (JsonObject)_jsonCode["blocks"]!;
        HashSet<CodeBlock> codeBlocks = new();
        foreach (var block in blocks)
        {
            if (block.Value?["topLevel"]?.GetValue<bool>() ?? false)
                codeBlocks.Add(new(blocks, (JsonObject)block.Value));
        }
        _blocks = codeBlocks.ToFrozenSet();
        _jsonCode = null!;
    }
}