using System.Text;

namespace ScratchCompiler.Parser.Operations;

public class OrOperator(LogicArgument a, LogicArgument b) : LogicArgument, IBracketedString
{
    public const string ScratchCode = "operator_or";

    public LogicArgument Left => a;
    public LogicArgument Right => b;
    
    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets)
    {
        StringBuilder sb = new();
        if (useBrackets)
            sb.Append('(');

        if (a is OrOperator or)
            sb.Append(or.ToString(false));
        else
            sb.Append(a);
        
        sb.Append(" or ");

        if (b is OrOperator orB)
            sb.Append(orB.ToString(false));
        else
            sb.Append(b);
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
    }
}