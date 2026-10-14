namespace ScratchCompiler.Decoder.Functions.Motion;

public class Rotate(float degrees) : Function
{
    public const string ScratchCodeLeft = "motion_turnleft";
    public const string ScratchCodeRight = "motion_turnright";
    
    public float Degrees => degrees;
}