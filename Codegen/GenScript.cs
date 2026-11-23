using System.Collections.Immutable;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Functions.Control;
using ScratchCompiler.Parser.Functions.Looks;
using ScratchCompiler.Parser.Functions.Motion;
using ScratchCompiler.Parser.Operations;

namespace ScratchCompiler.Codegen;

internal static class GenScript
{
    
    private static CodegenTools Current => Generator.Current;
    private static LLVMBuilderRef Builder => Generator.Current.Builder;

    // ReSharper disable once UseCollectionExpression
    public static readonly ImmutableArray<LLVMTypeRef> ScriptBodyArgs = new[] {
        Current.Libs.ActorStruct.Pointer, // Actor* actor
        Current.Libs.PointerType,         // void* scripts_internal_args
        Current.Libs.PointerType          // i32* point_in_script
    }.ToImmutableArray();
    
    public static readonly LLVMTypeRef ScriptBodyType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [..ScriptBodyArgs]);
    
    [ThreadStatic] private static LLVMMethod? _currentScript;
    [ThreadStatic] private static int _conStrCount;
    [ThreadStatic] private static Dictionary<uint, LLVMBasicBlockRef>? _breakpoints;
    [ThreadStatic] private static uint _stackSize;
    [ThreadStatic] private static uint? _waitArg;
    [ThreadStatic] private static uint? _glideArg;
    [ThreadStatic] private static LLVMBasicBlockRef? _pureExit;

    public static uint GenerateScript(TargetsCode target, CodeBlock script, LLVMMethod method)
    {
        ResetConstVarCounters();
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
        return _stackSize;

        void ResetConstVarCounters()
        {
            _conStrCount = 0;
            _breakpoints = [];
            _stackSize = 0u;
            _pureExit = null;

            _waitArg = null;
            _glideArg = null;
        }
        void BuildEntrySwitch()
        {
            Builder.PositionAtEnd(entryBlock);
            var @goto = Builder.BuildLoad2(LLVMTypeRef.Int32, method.Function.GetParam(2), "script_point");
            var @switch = Builder.BuildSwitch(@goto, exitBlock, (uint)_breakpoints!.Count);
            unsafe
            {
                foreach (var breakpoint in _breakpoints)
                    LLVM.AddCase(@switch, LLVMValueRef.CreateConst(breakpoint.Key), breakpoint.Value);
            }
        }
    }
    
    private static (LLVMBasicBlockRef block, uint index) CreateScriptBreakPoint()
    {
        var key = FindFreeBreakpoint();
        var block = _currentScript!.Function.AppendBasicBlock("Script #" + key);
        _breakpoints!.Add(key, block);
        return (block, key);
        uint FindFreeBreakpoint()
        {
            // ReSharper disable once VariableHidesOuterVariable
            uint key = 1;
            var keys = _breakpoints!.Keys;
            while (keys.Contains(key))
                ++key;
            return key;
        }
    }

    private static LLVMValueRef GetFieldInStack(uint offset)
    {
        if(offset == 0)
            return _currentScript!.Function.GetParam(1);
        
        return Builder.BuildGEP2(Current.Context.Int8Type, _currentScript!.Function.GetParam(1), [LLVMValueRef.CreateConst(offset)]);
    }
    private static void BuildBreakScriptRun(uint index)
    {
        Builder.BuildStore(LLVMValueRef.CreateConst(index), _currentScript!.Function.GetParam(2));
        Builder.BuildRetVoid();
    }
    
    private static void BuildFunctionBlock(TargetsCode target, Function function, string methodName)
    {
        switch (function)
        {
            #region Motion
                case MoveTo @goto: BuildGotoBlock(target, @goto, methodName); break;
                case GlideTo glide: BuildGlideToBlock(target, glide, methodName); break;
            #endregion
            #region Looks
                case Say say: BuildSayBlock(target, say, methodName); break;
                case Think think: BuildThinkBlock(target, think, methodName); break;
            #endregion
            #region Control
                case Wait wait: BuildWaitBlock(target, wait, methodName); break;
            #endregion
            
            default: throw Generator.NotImplemented(function);
        }
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
                case GetDouble.SystemSources.Timer:
                    return (Builder.BuildCall2(Current.Libs.SensingTimer, [], "timer"), NoDispose());
            }
        throw Generator.NotImplemented(get);
    }
    
    private static void BuildSayBlock(TargetsCode target, Say say, string methodName)
    {
        var text = GetStringArg(methodName, say.Text, target);
        Builder.BuildCall2(Current.Libs.LooksSay, [target.Name, text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }
    private static void BuildThinkBlock(TargetsCode target, Think think, string methodName)
    {
        var text = GetStringArg(methodName, think.Text, target);
        Builder.BuildCall2(Current.Libs.LooksThink, [target.Name, text.Value]);
        foreach (var @ref in text.toFree)
            Builder.BuildFree(@ref);
    }

    private static void BuildGlideToBlock(TargetsCode target, GlideTo glide, string methodName)
    {
        if (!_glideArg.HasValue)
        {
            _glideArg = _stackSize;
            _stackSize += sizeof(double) * 6;
        }

        var time = GetFloatArg(methodName, glide.Time, target);
        switch (glide.Destination)
        {
            case { IsT1: true, AsT1: var dest }: 
            {
                var x = GetFloatArg(methodName, dest.Item1, target);
                var y = GetFloatArg(methodName, dest.Item2, target);
                
                Builder.BuildCall2(Current.Libs.MotionBeginGlideTo, [_currentScript!.Function.GetParam(0), GetFieldInStack(_glideArg.Value), x.Value, y.Value, time.Value]);
                
                foreach (var @ref in x.toFree.Concat(y.toFree).Concat(time.toFree))
                    Builder.BuildFree(@ref);
            } break;
            default: throw Generator.NotImplemented(glide);
        }

        var breakpoint = CreateScriptBreakPoint();
        BuildBreakScriptRun(breakpoint.index);
        if (!_pureExit.HasValue)
        {
            _pureExit = _currentScript.Function.AppendBasicBlock("pure_exit");
            Builder.PositionAtEnd(_pureExit.Value);
            Builder.BuildRetVoid();
        }
        Builder.PositionAtEnd(breakpoint.block);
        var isDone = Builder.BuildCall2(Current.Libs.MotionGlideTo, [_currentScript.Function.GetParam(0), GetFieldInStack(_glideArg.Value)], "done");
        var @continue = _currentScript.Function.AppendBasicBlock("glide_to_continue");
        Builder.BuildCondBr(isDone, @continue, _pureExit.Value);
        Builder.PositionAtEnd(@continue);
    }
    
    private static void BuildGotoBlock(TargetsCode target, MoveTo @goto, string methodName)
    {
        switch (@goto.Destination)
        {
            case { IsT1: true, AsT1: var dest }:
            {
                var x = GetFloatArg(methodName, dest.Item1, target);
                var y = GetFloatArg(methodName, dest.Item2, target);

                var actor = _currentScript!.Function.GetParam(0);

                Builder.BuildCall2(Current.Libs.MotionMoveToXY, [actor, x.Value, y.Value]);
        
                foreach (var @ref in x.toFree.Concat(y.toFree))
                    Builder.BuildFree(@ref);  
            } return;
            case { IsT0: true, AsT0: var dest }:
            {
                switch (dest)
                {
                    case MoveTo.RelativePosition.Random: Builder.BuildCall2(Current.Libs.MotionMoveToRandom, [_currentScript!.Function.GetParam(0)]); return;
                }
            } break;
        }
        throw Generator.NotImplemented(@goto);
    }

    private static void BuildWaitBlock(TargetsCode target, Wait wait, string methodName)
    {
        if (_waitArg is null)
        {
            _waitArg = _stackSize;
            _stackSize += sizeof(double);
        }

        var delay = GetFloatArg(methodName, wait.Delay, target);
        var now = Builder.BuildCall2(Current.Libs.SensingTimer, [], "now");
        var waitTo = Builder.BuildFAdd(now, delay.Value, "wait_to");
        foreach (var @ref in delay.toFree)
            Builder.BuildFree(@ref);
        
        Builder.BuildStore(waitTo, GetFieldInStack(_waitArg.Value));
        
        var breakPoint = CreateScriptBreakPoint();
        BuildBreakScriptRun(breakPoint.index);
        
        if (!_pureExit.HasValue)
        {
            _pureExit = _currentScript!.Function.AppendBasicBlock("pure_exit");
            Builder.PositionAtEnd(_pureExit.Value);
            Builder.BuildRetVoid();
        }
        Builder.PositionAtEnd(breakPoint.block);
        var @continue = _currentScript!.Function.AppendBasicBlock("control_wait_continue");
        now = Builder.BuildCall2(Current.Libs.SensingTimer, [], "now");
        waitTo = Builder.BuildLoad2(Current.Context.DoubleType, GetFieldInStack(_waitArg.Value), "wait_to");
        var condition = Builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, now, waitTo);
        Builder.BuildCondBr(condition, @continue, _pureExit.Value);
        Builder.PositionAtEnd(@continue);
    }
    
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildAddOperator(TargetsCode target, AddOperator add, string methodName)
    {
        var valA = GetFloatArg(methodName, add.A, target);
        var valB = GetFloatArg(methodName, add.B, target);
        
        var rez = Builder.BuildFAdd(valA.Value, valB.Value, "add_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildSubtractOperator(TargetsCode target, SubtractOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFSub(valA.Value, valB.Value, "sub_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildMultiplyOperator(TargetsCode target, MultiplyOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFMul(valA.Value, valB.Value, "mul_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }
    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildDivideOperator(TargetsCode target, DivideOperator sub, string methodName)
    {
        var valA = GetFloatArg(methodName, sub.A, target);
        var valB = GetFloatArg(methodName, sub.B, target);
        
        var rez = Builder.BuildFDiv(valA.Value, valB.Value, "div_ope");

        foreach (var @ref in valA.toFree.Concat(valB.toFree))
            Builder.BuildFree(@ref);

        return (rez, NoDispose());
    }

    private static (LLVMValueRef Value, IEnumerable<LLVMValueRef> toFree) BuildJoinOperator(TargetsCode target, JoinString join, string methodName)
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