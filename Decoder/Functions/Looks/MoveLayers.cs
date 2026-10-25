using System.Collections.Frozen;
using Humanizer;

namespace ScratchCompiler.Decoder.Functions.Looks;

public class MoveLayers(MoveLayers.Direction direction, IntArgument layers) : Function
{
    public const string ScratchCode = "looks_goforwardbackwardlayers";
    
    public static readonly FrozenDictionary<string, Direction> DirectionMap =
        new Dictionary<string, Direction>
        {
            ["forward"] = Direction.Forward,
            ["backward"] = Direction.Backward,
        }.ToFrozenDictionary();

    public Direction InDirection => direction;
    public IntArgument Layers => layers;
    
    public override string ToString() => $"Move {direction.Humanize(LetterCasing.LowerCase)} {layers} layers;";

    public enum Direction : byte
    {
        Forward,
        Backward
    }
}