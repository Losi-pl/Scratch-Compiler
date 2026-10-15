namespace ScratchCompiler.Decoder.Functions.Motion;

public class MoveBySteps(float steps) : Function
{
    public const string ScratchCode = "motion_movesteps";
    
    public float Steps => steps;

    public override string ToString() => $"Move [{steps}] forward;";
}