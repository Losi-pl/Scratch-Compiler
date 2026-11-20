using System.Collections.Immutable;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Functions.Motion;
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

    public static uint GenerateScript(TargetsCode target, CodeBlock script, LLVMMethod method)
    {
        ResetConstVarCounters();
        Dictionary<uint, LLVMBasicBlockRef> breakpoints = [];
        
        var stackSize = 0u;
        _currentScript = method;
        var entryBlock = method.Function.AppendBasicBlock("entry");
        
        var scriptEntry = CreateScriptBreakPoint();
        Current.Builder.PositionAtEnd(scriptEntry.block);
        foreach (var operation in script.Methods)
            BuildFunctionBlock(target, operation, method.Function.Name);

        var exitBlock = method.Function.AppendBasicBlock("exit");
        Builder.BuildBr(exitBlock);
        Current.Builder.PositionAtEnd(exitBlock);
        BuildBreakScriptRun(0);
        
        BuildEntrySwitch();
        _currentScript = null!;
        return stackSize;

        (LLVMBasicBlockRef block, uint index) CreateScriptBreakPoint()
        {
            var key = FindFreeBreakpoint();
            var block = method.Function.AppendBasicBlock("Script #" + key);
            breakpoints.Add(key, block);
            return (block, key);
        }
        uint FindFreeBreakpoint()
        {
            uint key = 1;
            var keys = breakpoints.Keys;
            while (keys.Contains(key))
                ++key;
            return key;
        }

        void BuildEntrySwitch()
        {
            Builder.PositionAtEnd(entryBlock);
            var @goto = Builder.BuildLoad2(LLVMTypeRef.Int32, method.Function.GetParam(2), "script_point");
            var @switch = Builder.BuildSwitch(@goto, exitBlock, (uint)breakpoints.Count);
            unsafe
            {
                foreach (var breakpoint in breakpoints)
                    LLVM.AddCase(@switch, LLVMValueRef.CreateConst(breakpoint.Key), breakpoint.Value);
            }
        }
    }

    private static void BuildFunctionBlock(TargetsCode target, Function function, string methodName)
    {
        switch (function)
        {
            case Say say: BuildSayBlock(target, say, methodName); break;
            case Think think: BuildThinkBlock(target, think, methodName); break;
            case MoveTo @goto: BuildGotoBlock(target, @goto, methodName); break;
            default: throw Generator.NotImplemented(function);
        }
    }
    public static void BuildBreakScriptRun(uint index)
    {
        Builder.BuildStore(LLVMValueRef.CreateConst(index), _currentScript!.Function.GetParam(2));
        Builder.BuildRetVoid();
    }

    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetStringArg(string method, Argument arg, TargetsCode target)
    {
        switch (arg)
        {
            case ConstString @const: return (GetConstString(method, @const.Const), NoDispose());
            case JoinString join: return BuildJoinOperator(target, join, method);
            case IntArgument intArg: return CastIntToString(method, intArg, target);
            case FloatArgument floatArg: return CastDoubleToString(method, floatArg, target);
        }
        
        throw Generator.NotImplemented(arg);
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetFloatArg(string method, FloatArgument arg, TargetsCode target)
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
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) GetFloatValue(string _1, GetDouble get, TargetsCode _2)
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
                }
                case GetDouble.SystemSources.YPosition:
                {
                    var field = Builder.BuildStructGEP2(Current.Libs.ActorStruct, _currentScript.Function.GetParam(0), 2, "field: y");
                    var x = Builder.BuildLoad2(Current.Context.DoubleType, field);
                    return (x, NoDispose());
                }
            }
        throw Generator.NotImplemented(get);
    }
    
    public static void BuildSayBlock(TargetsCode target, Say say, string methodName)
    {
        var text = GetStringArg(methodName, say.Text, target);
        Builder.BuildCall2(Current.Libs.LooksSay, [target.Name, text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }
    public static void BuildThinkBlock(TargetsCode target, Think think, string methodName)
    {
        var text = GetStringArg(methodName, think.Text, target);
        Builder.BuildCall2(Current.Libs.LooksThink, [target.Name, text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }

    public static void BuildGotoBlock(TargetsCode target, MoveTo @goto, string methodName)
    {
        if (@goto.Destination.IsT0)
            throw Generator.NotImplemented(@goto);

        var x = GetFloatArg(methodName, @goto.Destination.AsT1.Item1, target);
        var y = GetFloatArg(methodName, @goto.Destination.AsT1.Item2, target);

        var actor = _currentScript!.Function.GetParam(0);

        Builder.BuildCall2(Current.Libs.MotionMoveToXY, [actor, x.Value, y.Value]);
        
        foreach (var @ref in x.toFree.Concat(y.toFree))
            Builder.BuildFree(@ref);
    }

    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildAddOperator(TargetsCode target, AddOperator add, string methodName)
    {
        var valA = GetFloatArg(methodName, add.A, target);
        var valB = GetFloatArg(methodName, add.B, target);
        
        var rez = Builder.BuildFAdd(valA.Value, valB.Value, "add_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildSubtractOperator(TargetsCode target, SubtractOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFSub(valA.Value, valB.Value, "sub_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildMultiplyOperator(TargetsCode target, MultiplyOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFMul(valA.Value, valB.Value, "mul_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildDivideOperator(TargetsCode target, DivideOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFDiv(valA.Value, valB.Value, "div_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }

    public static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildJoinOperator(TargetsCode target, JoinString join, string methodName)
    {
        if (join.IsConstant)
            return (GetConstString(methodName, join.AsConstant), NoDispose());
        
        var str1 = GetStringArg(methodName, join.Left, target);
        var str2 = GetStringArg(methodName, join.Right, target);

        var rez = Builder.BuildCall2(Generator.Current.Libs.OperatorJoin, [str1.Value, str2.Value], "join_ope");
        
        foreach (var @ref in str1.toFree.Concat(str2.toFree))
            Builder.BuildFree(@ref);
        
        return (rez, [rez]);
    }
    
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) CastIntToString(string method, IntArgument arg, TargetsCode target) 
    {
        throw new NotImplementedException("Not implemented");
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) CastDoubleToString(string method, FloatArgument arg, TargetsCode target) 
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