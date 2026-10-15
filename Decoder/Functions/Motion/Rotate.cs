namespace ScratchCompiler.Decoder.Functions.Motion;

public class Rotate(float degrees, Rotate.TurnDirection direction) : Function
{
    public const string ScratchCodeLeft = "motion_turnleft";
    public const string ScratchCodeRight = "motion_turnright";
    
    public float Degrees => degrees;
    public TurnDirection Direction => direction;

    public override string ToString() => $"Rotate [{degrees}] to the {direction.ToString().ToLower()};";
    
    public enum TurnDirection : byte
    {
        Left, Right
    }
}