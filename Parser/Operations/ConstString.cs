using Humanizer;

namespace ScratchCompiler.Parser.Operations;

public class ConstString(string @const) : Argument, IConstArgument
{
    public string Const => @const;
    
    public override string ToString() => $"\"{@const.Truncate(20)}\"";
}