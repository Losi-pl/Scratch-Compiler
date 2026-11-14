using System.Collections.Immutable;
using System.Globalization;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Codegen;

internal static class GenScript
{
    private static int _conStrCount;

    // ReSharper disable once UseCollectionExpression
    public static readonly ImmutableArray<LLVMTypeRef> ScriptBodyArgs = new[] {
        Current.Libs.ActorStruct.Pointer, // Actor* actor
        Current.Libs.PointerType,         // void* scripts_internal_args
        Current.Libs.PointerType          // i32* point_in_script
    }.ToImmutableArray();
    
    public static readonly LLVMTypeRef ScriptBodyType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [..ScriptBodyArgs]);
    
    public static void ResetConstVarCounters()
    {
        _conStrCount = 0;
    }
    
    private static CodegenTools Current => Generator.Current;
    private static LLVMBuilderRef Builder => Generator.Current.Builder;
    [ThreadStatic] private static LLVMMethod? _currentScript;

    public static uint GenerateScript(Target target, CodeBlock script, LLVMMethod method)
    {
        ResetConstVarCounters();
        
        var stackSize = 0u;
        _currentScript = method;
        var currentBlock = method.Function.AppendBasicBlock("entry");
        Current.Builder.PositionAtEnd(currentBlock);
        
        foreach (var operation in script.Methods)
        {
            switch (operation)
            {
                case Say say: BuildSayBlock(target, say, method.Function.Name); break;
                case Think think: BuildThinkBlock(target, think, method.Function.Name); break;
                default: throw Generator.NotImplemented(method);
            }  
        }

        Builder.BuildRetVoid();
        _currentScript = null!;
        return stackSize;
    }
    
    public static void BuildSayBlock(Target target, Say say, string methodName)
    {
        var text = GetStringArg(methodName, say.Text, target);
        Builder.BuildCall2(Current.Libs.LooksSay, [Current.TargetNames[target], text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }
    public static void BuildThinkBlock(Target target, Think think, string methodName)
    {
        var text = GetStringArg(methodName, think.Text, target);
        Builder.BuildCall2(Current.Libs.LooksThink, [Current.TargetNames[target], text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }

    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildAddOperator(Target target, AddOperator add, string methodName)
    {
        var valA = GetFloatArg(methodName, add.A, target);
        var valB = GetFloatArg(methodName, add.B, target);
        
        var rez = Builder.BuildFAdd(valA.Value, valB.Value, "add_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildSubtractOperator(Target target, SubtractOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFSub(valA.Value, valB.Value, "sub_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildMultiplyOperator(Target target, MultiplyOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFMul(valA.Value, valB.Value, "mul_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildDivideOperator(Target target, DivideOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFDiv(valA.Value, valB.Value, "div_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetStringArg(string method, Argument arg, Target target)
    {
        switch (arg)
        {
            case ConstString @const: return (GetConstString(method, @const.Const), NoDispose());
            case IntArgument intArg: return CastIntToString(method, intArg, target);
            case FloatArgument floatArg: return CastDoubleToString(method, floatArg, target);
        }
        
        throw Generator.NotImplemented(arg);
    }

    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetFloatArg(string method, FloatArgument arg, Target target)
    {
        switch (arg)
        {
            case ConstDouble floatArg: return (LLVMValueRef.CreateConst(floatArg.Const), NoDispose());
            case ConstInt intArg: return (LLVMValueRef.CreateConst((float)intArg.Const), NoDispose());
            case AddOperator add: return BuildAddOperator(target, add, method);
            case SubtractOperator sub: return BuildSubtractOperator(target, sub, method);
            case DivideOperator div: return BuildDivideOperator(target, div, method);
            case MultiplyOperator mul: return BuildMultiplyOperator(target, mul, method);
            case GetDouble get: return GetFloatValue(method, get, target);
            default: throw Generator.NotImplemented(arg);
        }
    }

    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetFloatValue(string method, GetDouble get, Target target)
    {
        if (_currentScript is null)
            throw new InvalidOperationException();
        if(get.FromSystem)
            switch (get.SystemSource)
            {
                case GetDouble.SystemSources.XPosition:
                {
                    var field = Builder.BuildStructGEP2(Current.Libs.ActorStruct, _currentScript.Function.GetParam(0), 1, "field: x");
                    var x = Builder.BuildLoad2(Current.Context.DoubleType, field);
                    return (x, NoDispose());
                } break;
                case GetDouble.SystemSources.YPosition:
                {
                    var field = Builder.BuildStructGEP2(Current.Libs.ActorStruct, _currentScript.Function.GetParam(0), 2, "field: y");
                    var x = Builder.BuildLoad2(Current.Context.DoubleType, field);
                    return (x, NoDispose());
                } break;
            }
        throw Generator.NotImplemented(get);
    }
    
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) CastIntToString(string method, FloatArgument arg, Target target) 
    {
        throw new Exception("Not implemented");
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) CastDoubleToString(string method, FloatArgument arg, Target target) 
    {
        var value = GetFloatArg(method, arg, target);
        
        var rez = Builder.BuildCall2(Generator.Current.Libs.CastDoubleToString, [value.Value]);

        foreach (var @ref in value.toFree)
            Builder.BuildFree(@ref);
        
        return (rez, [rez]);
    }

    private static IEnumerable<LLVMValueRef> NoDispose() => Enumerable.Empty<LLVMValueRef>();
    private static CharPointer GetConstString(string method, string content) => Generator.Current.Builder.BuildGlobalString(content, $"{method}::str_{_conStrCount++:00}");
}