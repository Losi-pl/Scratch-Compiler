using Humanizer;

namespace ScratchCompiler.Parser.Operations;

public class UnknownStringArgument(string opcode, string @const) : Argument
{
    public string Const => @const;
    
    public override string ToString() => $"[{@const.Truncate(20)}]";
}