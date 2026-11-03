using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Codegen;

internal static class GenMethodCalls
{
    private static int _conStrCount;

    private static readonly LLVMTypeRef _scriptsFunctionType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, []);
    
    public static void ResetConstVarCounters()
    {
        _conStrCount = 0;
    }
    
    private static CodegenTools Current => Generator.Current;
    private static LLVMBuilderRef Builder => Generator.Current.Builder;

    public static uint GenerateScript(Target target, CodeBlock script, LLVMMethod method)
    {
        ResetConstVarCounters();
        
        var stackSize = 0u;
        var currentBlock = method.Function.AppendBasicBlock("entry");
        Current.Builder.PositionAtEnd(currentBlock);
        
        foreach (var operation in script.Methods)
        {
            switch (operation)
            {
                case Say say:
                    BuildSayBlock(target, say, method.Function.Name);
                    break;
                case Think think:
                    BuildThinkBlock(target, think, method.Function.Name);
                    break;
                        
                default:
                    throw Generator.NotImplemented(method);
                
            }  
        }

        Builder.BuildRetVoid();
        return stackSize;
    }
    
    public static void BuildSayBlock(Target target, Say say, string methodName)
    {
        CharPointer text;
        if (say.Text is ConstString @const)
            text = GetConstString(methodName, @const.Const);
        else
            throw Generator.NotImplemented(say.Text);
        
        Builder.BuildCall2(Current.Libs.LooksSay, [Current.TargetNames[target], text]);
    }
    
    public static void BuildThinkBlock(Target target, Think think, string methodName)
    {
        CharPointer text;
        if (think.Text is ConstString @const)
            text = GetConstString(methodName, @const.Const);
        else
            throw Generator.NotImplemented(think.Text);
                            
        Builder.BuildCall2(Current.Libs.LooksThink, [Current.TargetNames[target], text]);
    }
    
    
    private static CharPointer GetConstString(string method, string content) => Generator.Current.Builder.BuildGlobalString(content, $"{method}::str_{_conStrCount++:00}");
}