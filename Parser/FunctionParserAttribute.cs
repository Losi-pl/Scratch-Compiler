namespace ScratchCompiler.Parser;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal class FunctionParserAttribute(string opcode) : Attribute
{
    public string OpCode => opcode;
}