using System.Collections.Frozen;
using LLVMSharp.Interop;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Entries;

namespace ScratchCompiler.Codegen;

partial class Generator
{
    private static void DeclareScriptMethods(SyntaxTree tree)
    {
        var engineType = LLVMTypeRef.CreateFunction(Current.Context.VoidType, [Current.Libs.ActorStruct.Pointer]);
        Dictionary<Target, ((CodeBlock script, LLVMMethod method, bool defined, uint stackSize)[] scripts, LLVMMethod engine)> scriptList = [];
        
        foreach (var target in tree.GetAllTargets())
        {
            var onStarts = 0u;
            var onClick = 0u;
            var onBackgroundChange = 0u;
            var onBroadcast = 0u;
            var onCloned = 0u;
            var onGraterThan = 0u;
            var onKeyPress = 0u;

            List<(CodeBlock script, LLVMMethod method, bool defined, uint stackSize)> scripts = [];
            
            foreach (var script in target.Blocks)
            {
                if(script.Entry is Dormant)
                    continue;

                var name = $"{target.Name}::{script.Entry switch {
                    OnStart=> $"OnStart_{onStarts++}",
                    OnKeyPress => $"OnKeyPress_{onKeyPress++}",
                    OnBroadcast => $"OnBroadcast_{onBroadcast++}",
                    OnActorClick => $"OnClick_{onClick++}",
                    OnStageClick => $"OnClick_{onClick++}",
                    OnBackgroundChangedTo => $"OnBackgroundChange_{onBackgroundChange++}",
                    OnCloneCreated => $"OnCloned_{onCloned++}",
                    OnGraterThan => $"OnGraterThan_{onGraterThan++}",
                    _ => throw NotImplemented(script.Entry)
                }}";
                
                var scriptFun = Current.Module.AddFunction(name, GenScript.ScriptBodyType);
                scripts.Add((script, new(scriptFun, GenScript.ScriptBodyType, Current.Context.VoidType, [..GenScript.ScriptBodyArgs], false), false, 0));
                
                scriptFun.GetParam(0).Name = "actor";
                scriptFun.GetParam(1).Name = "scripts_stack";
                scriptFun.GetParam(2).Name = "goto_point";
            }
            
            var engineFun = Current.Module.AddFunction($"{target.Name}::Engine", engineType);
            
            {
                engineFun.GetParam(0).Name = "Actor";
                engineFun.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeFunctionIndex, Current.Libs.HotAttribute);
                var blocks = scripts.Select((m, i) =>
                {
                    var name = m.method.Function.Name;
                    name = name[(name.IndexOf(':') + 2)..];
                    var test = i == 0 ? "entry" : name + "::Test";
                    var call = name + "::Call";
                    return (Test: engineFun.AppendBasicBlock(test), Call: engineFun.AppendBasicBlock(call));
                }).ToArray();
                if (blocks.Length == 0)
                {
                    var block = engineFun.AppendBasicBlock("entry");
                    Current.Builder.PositionAtEnd(block);
                    Current.Builder.BuildRetVoid();
                }
                else
                {
                    var exit = engineFun.AppendBasicBlock("exit");
                    Current.Builder.PositionAtEnd(blocks[0].Test);
                    var scriptField = Current.Builder.BuildStructGEP2(Current.Libs.ActorStruct, engineFun.FirstParam, 4);
                    var scriptsPtr = Current.Builder.BuildLoad2(Current.Libs.ScriptHolder.Pointer, scriptField, "Scripts");

                    for (uint i = 0; i < scripts.Count; i++)
                    {
                        var @goto = Current.Builder.BuildGEP2(Current.Libs.ScriptHolder, scriptsPtr, stackalloc[] {LLVMValueRef.CreateConst(i), LLVMValueRef.CreateConst(0)}, $"Script[{i}]::GOTO");
                        var gotoVal = Current.Builder.BuildLoad2(Current.Context.Int32Type, @goto);
                        var isRunning = Current.Builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, gotoVal, LLVMValueRef.CreateConst<uint>(0), $"Script[{i}]::Running");
                        Current.Builder.BuildCondBr(isRunning, blocks[i].Call, i != scripts.Count - 1 ? blocks[i + 1].Test : exit);
                        
                        Current.Builder.PositionAtEnd(blocks[i].Call);
                        var scriptPtr = Current.Builder.BuildGEP2(Current.Libs.ScriptHolder, scriptsPtr, stackalloc[] {LLVMValueRef.CreateConst(i)}, $"Script[{i}]");
                        var stackPtr = Current.Builder.BuildStructGEP2(Current.Libs.ScriptHolder, scriptPtr, 2);
                        var stack = Current.Builder.BuildLoad2(Current.Libs.PointerType, stackPtr, $"Script[{i}]::Stack");
                        Current.Builder.BuildCall2(scripts[(int)i].method, [engineFun.GetParam(0), stack, @goto]);
                        Current.Builder.BuildBr(i != scripts.Count - 1 ? blocks[i + 1].Test : exit);
                        if(i != scripts.Count - 1)
                            Current.Builder.PositionAtEnd(blocks[i + 1].Test);
                    }
                    Current.Builder.PositionAtEnd(exit);
                    Current.Builder.BuildRetVoid();
                }
            }
            
            scriptList[target] = (scripts.ToArray(), new(engineFun, engineType, Current.Context.VoidType, [Current.Libs.ActorStruct.Pointer], false));
        }

        Current.Scripts = scriptList.ToFrozenDictionary();
    }
}