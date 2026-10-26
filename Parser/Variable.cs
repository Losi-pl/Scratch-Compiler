namespace ScratchCompiler.Parser;

public class Variable(string name, Target target, object defaultValue)
{
    public string Name => name;
    public bool IsGlobal => target is Scene;
    public bool IsLocal => target is Actor;
    public object DefaultValue => defaultValue;
    
    public Actor Actor => target as Actor ?? throw new InvalidOperationException("This Variable is Global");
}