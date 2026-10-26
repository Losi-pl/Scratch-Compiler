using System.Drawing;

namespace ScratchCompiler.Parser.Operations.Sensing;

public class ColorTouchingColor(Color a, Color b) : LogicArgument
{
    public const string ScratchCode = "sensing_coloristouchingcolor";

    public override string ToString() => $"(#{a.R:X2}{a.G:X2}{a.B:X2}) is touching (#{b.R:X2}{b.G:X2}{b.B:X2})";
}