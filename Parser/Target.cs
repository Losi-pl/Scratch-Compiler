using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public abstract class Target
{
    private JsonObject _jsonCode;
    private FrozenSet<CodeBlock> _blocks = null!;
    private FrozenDictionary<string, Broadcast> _broadcasts;
    private FrozenDictionary<string, Variable> _variables;

    internal Target(JsonObject json)
    {
        _jsonCode = json;
        Name = json["name"]!.GetValue<string>();
        DefaultCostume = (uint)(json["currentCostume"]?.GetValue<int>() ?? 0);
        Costumes = GetCostumes(json["costumes"]!.AsArray());
        Sounds = GetSounds(json["sounds"]!.AsArray());
        _broadcasts = json["broadcasts"]!.AsObject().ToFrozenDictionary(kvp => kvp.Key,
            kvp => new Broadcast(kvp.Value?.GetValue<string>() ?? string.Empty));
        _variables = GetVariables(json["variables"]!.AsObject());
    }

    public string Name { get; }
    public FrozenSet<CodeBlock> Blocks => _blocks;
    public ImmutableArray<Costume> Costumes { get; }
    public ImmutableArray<Sound> Sounds { get; }
    public readonly uint DefaultCostume;
    public ImmutableArray<Broadcast> Broadcasts => _broadcasts.Values;
    public ImmutableArray<Variable> Variables => _variables.Values;

    private ImmutableArray<Costume> GetCostumes(JsonArray json)
    {
        List<Costume> costumes = new();

        foreach (var costume in json.Cast<JsonObject>())
            costumes.Add(new(costume));
        
        // ReSharper disable once UseCollectionExpression
        return costumes.ToImmutableArray();
    }
    
    private ImmutableArray<Sound> GetSounds(JsonArray json)
    {
        List<Sound> sounds = new();

        foreach (var sound in json.Cast<JsonObject>())
            sounds.Add(new(sound));
        
        // ReSharper disable once UseCollectionExpression
        return sounds.ToImmutableArray();
    }

    private FrozenDictionary<string, Variable> GetVariables(JsonObject json) => json.ToFrozenDictionary(kvp => kvp.Key, kvp =>
    {
        var name = kvp.Value![0]!.GetValue<string>();
        var jsonVal = kvp.Value![1]?.AsValue();

        if (jsonVal is not null)
        {
            switch (jsonVal.GetValueKind())
            {
                case JsonValueKind.Number:
                {
                    var code = jsonVal.ToJsonString();
                    if(int.TryParse(code, out var i))
                        return new Variable(name, this, i);
                    if(float.TryParse(code, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                        return new Variable(name, this, f);
                        
                    return new Variable(name, this, float.NaN);
                }
                case JsonValueKind.String:
                    return new Variable(name, this, jsonVal.GetValue<string>());
                case JsonValueKind.True or JsonValueKind.False:
                    return new Variable(name, this, jsonVal.GetValueKind() == JsonValueKind.True);
            }
        }    
        return new Variable(name, this, null!);
    });

    internal Broadcast? GetBroadcastByCode(string code) => (_broadcasts?.TryGetValue(code, out var b) ?? false) ? b : null;
    internal Variable? GetVariableByCode(string code) => (_variables?.TryGetValue(code, out var b) ?? false) ? b : null;
    internal void LoadCode()
    {
        var blocks = (JsonObject)_jsonCode["blocks"]!;
        HashSet<CodeBlock> codeBlocks = new();
        foreach (var block in blocks)
        {
            if (block.Value?["topLevel"]?.GetValue<bool>() ?? false)
                codeBlocks.Add(new(blocks, (JsonObject)block.Value, this));
        }
        _blocks = codeBlocks.ToFrozenSet();
        _jsonCode = null!;
    }
}