namespace ScratchCompiler.Parser.Functions.Control;

public class CloneMyself : Function
{
    public const string ScratchCode = "control_create_clone_of";
    
    public static readonly CloneMyself Instance = new();

    public override string ToString() => "Clone this actor;";
}