using System.Text;

namespace ScratchCompiler.Parser.Operations;

public class JoinString(Argument left, Argument right) : Argument, IBracketedString
{
    public const string ScratchCode = "operator_join";

    public Argument Left => left;
    public Argument Right => right;

    public bool IsConstant 
    {
        get
        {
            // ReSharper disable LocalVariableHidesPrimaryConstructorParameter
            var left = Left switch
            {
                ConstString => true,
                JoinString j => j.IsConstant,
                _ => false
            };
            var right = Right switch
            {
                ConstString => true,
                JoinString j => j.IsConstant,
                _ => false
            };
            return left && right;
            // ReSharper restore LocalVariableHidesPrimaryConstructorParameter
        }
    }

    public string AsConstant => left switch {
        ConstString @const => @const.Const,
        JoinString join => join.AsConstant,
        _ => throw new InvalidOperationException("This is not a constant operator")
    } + right switch {
        ConstString @const => @const.Const,
        JoinString join => join.AsConstant,
        _ => throw new InvalidOperationException("This is not a constant operator")
    };
    
    public override string ToString() => ToString(true);
    public string ToString(bool useBrackets)
    {
        StringBuilder sb = new();
        if (useBrackets)
            sb.Append('(');

        if (left is JoinString join)
            sb.Append(join.ToString(false));
        else
            sb.Append(left);
        
        sb.Append("..");

        if (right is JoinString joinB)
            sb.Append(joinB.ToString(false));
        else
            sb.Append(right);
        
        if (useBrackets)
            sb.Append(')');
        return sb.ToString();
    }
}