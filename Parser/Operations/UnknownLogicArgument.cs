namespace ScratchCompiler.Parser.Operations;

public class UnknownLogicArgument(string opcode) : LogicArgument
{
    public string OpCode => opcode;
    
    public override string ToString() => "[False]";
}