using System.Globalization;
using System.Text;

namespace ScratchCompiler.Parser.Operations;

public class MultiplyOperator(FloatArgument a, FloatArgument b) : FloatArgument, IBracketedString
{
    public const string ScratchCode = "operator_multiply";
    private const string Null = "Null";
    
    public FloatArgument A => a;
    public FloatArgument B => b;

    public override string ToString() => ToString(true);

    public string ToString(bool useBrackets)
    {
        StringBuilder sb = new();
        if (useBrackets)
            sb.Append('(');

        {
            switch (a)
            {
                case IConstArgument and IBracketedString bs: sb.Append(bs.ToString(false)); break;
                case MultiplyOperator or  DivideOperator: sb.Append(((IBracketedString)a).ToString(false)); break;
                default: sb.Append(a); break;
            }
        }
        
        sb.Append(" * ");

        {
            switch (b)
            {
                case IConstArgument and IBracketedString bs: sb.Append(bs.ToString(false)); break;
                case MultiplyOperator mul: sb.Append(mul.ToString(false)); break;
                default: sb.Append(b); break;
            }
        }
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
        
    }
}