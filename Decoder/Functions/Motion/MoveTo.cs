using OneOf;

namespace ScratchCompiler.Decoder.Functions.Motion;

public class MoveTo : Function
{
    public const string ScratchCodeRelative = "motion_goto";
    public const string ScratchCodeAbsolute = "motion_gotoxy";
    
    public const string ScratchSelectionCode = "motion_goto_menu";
    

    public readonly OneOf<RelativePosition, (float, float)> Destination;
    
    public MoveTo(RelativePosition target) => Destination = target;
    public MoveTo((float x, float y) target) => Destination = target;
    public MoveTo(float x, float y) => Destination = (x, y);
    
    public enum RelativePosition : byte
    {
        Random, Mouse
    }
}