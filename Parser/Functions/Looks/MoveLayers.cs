using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Humanizer;

namespace ScratchCompiler.Parser.Functions.Looks;

public class MoveLayers : Function
{
    public const string ScratchCode = "looks_goforwardbackwardlayers";
    
    public static readonly FrozenDictionary<string, Direction> DirectionMap =
        new Dictionary<string, Direction>
        {
            ["forward"] = Direction.Forward,
            ["backward"] = Direction.Backward,
        }.ToFrozenDictionary();

    public Direction InDirection { get; }
    public IntArgument Layers { get; }

    private MoveLayers(Direction direction, IntArgument layers) => (InDirection, Layers) = (direction, layers);
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static MoveLayers Parse(JsonObject json, JsonObject list, Target _) {
        var direction = json.GetField("FORWARD_BACKWARD");
        var move = json.GetIntInput(list, "NUM");
        return new MoveLayers(DirectionMap[direction], move);
    }
    
    public override string ToString() => $"Move {InDirection.Humanize(LetterCasing.LowerCase)} {Layers} layers;";

    public enum Direction : byte
    {
        Forward,
        Backward
    }
}