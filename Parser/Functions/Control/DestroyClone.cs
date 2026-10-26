namespace ScratchCompiler.Parser.Functions.Control;

public class DestroyClone : Function
{
    public const string ScratchCode = "control_delete_this_clone";
    
    public static readonly DestroyClone Instance = new();

    public override string ToString() => "Destroy this clone;";
}