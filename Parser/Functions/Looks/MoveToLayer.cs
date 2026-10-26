using System.Collections.Frozen;
using Humanizer;

namespace ScratchCompiler.Parser.Functions.Looks;

public class MoveToLayer(MoveToLayer.Destination destination) : Function
{
    public const string ScratchCode = "looks_gotofrontback";

    public static readonly FrozenDictionary<string, Destination> DestinationMap = 
        Enum.GetValues<Destination>().Select(d => new KeyValuePair<string, Destination>(d.ToString().ToLower(), d)).ToFrozenDictionary();

    public override string ToString() => $"Go to {destination.Humanize(LetterCasing.LowerCase)} layer;";

    public enum Destination : byte
    {
        Front, Back
    }
}