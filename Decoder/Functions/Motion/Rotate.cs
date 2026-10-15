namespace ScratchCompiler.Decoder.Functions.Motion;

public class Rotate(float degrees, Rotate.TurnDirection direction) : Function
{
    public const string ScratchCodeLeft = "motion_turnleft";
    public const string ScratchCodeRight = "motion_turnright";
    
    public float Degrees => degrees;
    
    public enum TurnDirection : byte
    {
        Left, Right
    }
}