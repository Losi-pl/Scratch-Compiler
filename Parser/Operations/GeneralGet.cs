namespace ScratchCompiler.Parser.Operations;

public class GeneralGet(Variable variable) : Argument
{
    public Variable Variable => variable;

    public override string ToString() => $"{{{Variable.Name}}}";
}