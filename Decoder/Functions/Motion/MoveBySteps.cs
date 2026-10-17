namespace ScratchCompiler.Decoder.Functions.Motion;

public class MoveBySteps(FloatArgument steps) : Function
{
    public const string ScratchCode = "motion_movesteps";
    
    public FloatArgument Steps => steps;

    public override string ToString() => $"Move {steps} steps forward;";
}