using System.Drawing;

namespace ScratchCompiler.Parser.Operations.Sensing;

public class IsTouchingColor(Color color) : LogicArgument
{
    public const string ScratchCode = "sensing_touchingcolor";

    public override string ToString() => $"{{Is touching (#{color.R:X2}{color.G:X2}{color.B:X2})}}";
}