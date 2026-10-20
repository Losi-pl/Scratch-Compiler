using System.Globalization;
using System.Text;

namespace ScratchCompiler.Decoder.Operations;

public class AddOperator(FloatArgument a, FloatArgument b) : FloatArgument, IBracketedString
{
    public const string ScratchCode = "operator_add";
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
            if (a is IConstArgument)
                if(a is IBracketedString bs)
                    sb.Append(bs.ToString(false));
                else
                    sb.Append(a);
            else if(a is AddOperator or SubtractOperator)
                sb.Append(((IBracketedString)a).ToString(false));
            else
                sb.Append(a);
        }
        
        sb.Append(" + ");
        
        {
            if (b is IConstArgument)
                if(b is IBracketedString bs)
                    sb.Append(bs.ToString(false));
                else
                    sb.Append(b);
            else if(b is AddOperator add)
                sb.Append(add.ToString(false));
            else
                sb.Append(b);
        }
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
        
    }

    private static string CustomNumerics(FloatArgument arg) => arg switch
    {
        ConstFloat fl => fl.Const.ToString(CultureInfo.InvariantCulture),
        ConstInt @in => @in.Const.ToString(),
        null => Null,
        _ => arg.ToString()
    } ?? Null;
}