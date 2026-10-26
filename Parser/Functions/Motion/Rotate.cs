namespace ScratchCompiler.Parser.Functions.Motion;

public class Rotate(FloatArgument degrees, Rotate.TurnDirection direction) : Function
{
    public const string ScratchCodeLeft = "motion_turnleft";
    public const string ScratchCodeRight = "motion_turnright";
    
    public FloatArgument Degrees => degrees;
    public TurnDirection Direction => direction;

    public override string ToString() => $"Rotate {degrees} degrees to the {direction.ToString().ToLower()};";
    
    public enum TurnDirection : byte
    {
        Left, Right
    }
}