using System.Globalization;
using System.Text;

namespace ScratchCompiler.Parser.Operations;

public class SubtractOperator(FloatArgument a, FloatArgument b) : FloatArgument, IBracketedString
{
    public const string ScratchCode = "operator_subtract";
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
            if (a is IConstArgument and IBracketedString bs)
                sb.Append(bs.ToString(false));
            else if(a is AddOperator or SubtractOperator)
                sb.Append(((IBracketedString)a).ToString(false));
            else
                sb.Append(a);
        }
        
        sb.Append(" - ");
        
        {
            if (b is IConstArgument and IBracketedString bs)
                sb.Append(bs.ToString(false));
            else
                sb.Append(b);
        }
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
        
    }
}