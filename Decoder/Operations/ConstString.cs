using Humanizer;

namespace ScratchCompiler.Decoder.Operations;

public class ConstString(string @const) : Argument
{
    public string Const => @const;
    
    public override string ToString() => $"\"{@const.Truncate(20)}\"";
}