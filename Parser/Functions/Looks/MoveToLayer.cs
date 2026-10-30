using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Humanizer;

namespace ScratchCompiler.Parser.Functions.Looks;

public class MoveToLayer : Function
{
    public const string ScratchCode = "looks_gotofrontback";

    public static readonly FrozenDictionary<string, Destination> DestinationMap = 
        Enum.GetValues<Destination>().Select(d => new KeyValuePair<string, Destination>(d.ToString().ToLower(), d)).ToFrozenDictionary();

    public Destination Direction { get; }

    private MoveToLayer(Destination destination) => Direction = destination;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static MoveToLayer Parse(JsonObject json, JsonObject list, Target _) {
        var destination = json.GetField("FRONT_BACK");
        return new MoveToLayer(DestinationMap[destination]);
    }
    
    public override string ToString() => $"Go to {Direction.Humanize(LetterCasing.LowerCase)} layer;";

    public enum Destination : byte
    {
        Front, Back
    }
}