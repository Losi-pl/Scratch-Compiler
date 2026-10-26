using System.Text;

namespace ScratchCompiler.Parser.Operations;

public class AndOperator(LogicArgument a, LogicArgument b) : LogicArgument, IBracketedString
{
    public const string ScratchCode = "operator_and";

    public LogicArgument Left => a;
    public LogicArgument Right => b;
    
    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets)
    {
        StringBuilder sb = new();
        if (useBrackets)
            sb.Append('(');

        if (a is AndOperator and)
            sb.Append(and.ToString(false));
        else
            sb.Append(a);
        
        sb.Append(" and ");

        if (b is AndOperator andB)
            sb.Append(andB.ToString(false));
        else
            sb.Append(b);
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
    }
}