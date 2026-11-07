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
        Dictionary<Target, (ScriptData[] scripts, TargetEvents events)> scriptList = [];
        
        foreach (var target in tree.GetAllTargets())
        {
            var onStarts = 0u;
            var onClick = 0u;
            var onBackgroundChange = 0u;
            var onBroadcast = 0u;
            var onCloned = 0u;
            var onGraterThan = 0u;
            var onKeyPress = 0u;

            List<ScriptData> scripts = [];
            
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
                scripts.Add(new(script, new(scriptFun, GenScript.ScriptBodyType, Current.Context.VoidType, 
                    [..GenScript.ScriptBodyArgs], false), false, 0));
                
                scriptFun.GetParam(0).Name = "actor";
                scriptFun.GetParam(1).Name = "scripts_stack";
                scriptFun.GetParam(2).Name = "goto_point";
            }

            var engineFun = GenScriptsUpdate();

            var eventStruct = GenEventsStruct(engineFun, null!);
            scriptList[target] = (scripts.ToArray(), new(engineFun, null!, eventStruct));


            LLVMValueRef GenEventsStruct(LLVMMethod? update, LLVMMethod? onStart)
            {
                unsafe
                {
                    fixed (LLVMValueRef* fields = stackalloc LLVMValueRef[2])
                    {
                        fields[0] = update?.Function ?? LLVMValueRef.CreateConstPointerNull(Current.Libs.PointerType);
                        fields[1] = onStart?.Function ?? LLVMValueRef.CreateConstPointerNull(Current.Libs.PointerType);
                        
                        var structVal = LLVM.ConstNamedStruct(Current.Libs.ScriptEvents.Type, (LLVMOpaqueValue**)fields, 2);
                        using var name = new MarshaledString($"{target.Name}::Events");
                        var global = LLVM.AddGlobal(Current.Module, Current.Libs.ScriptEvents.Type, name);
                        LLVM.SetInitializer(global, structVal);
                        
                        LLVM.SetLinkage(global, LLVMLinkage.LLVMLinkerPrivateLinkage);           // private
                        LLVM.SetUnnamedAddr(global, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr); // unnamed_addr
                        LLVM.SetGlobalConstant(global, 1 /* true */);                   // constant
                        LLVM.SetAlignment(global, 1);                                      // align 1

                        return global;
                    }
                }
            }
            LLVMMethod GenScriptsUpdate() 
            {
                // ReSharper disable once VariableHidesOuterVariable
                var engineFun = Current.Module.AddFunction($"{target.Name}::Engine", engineType);
                
                engineFun.GetParam(0).Name = "Actor";
                engineFun.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeFunctionIndex, Current.Libs.HotAttribute);
                var blocks = scripts.Select((m, i) =>
                {
                    var name = m.Function.Function.Name;
                    name = name[(name.IndexOf(':') + 2)..];
                    var test = i == 0 ? "entry" : name + "::Test";
                    var call = name + "::Call";
                    return (Test: engineFun.AppendBasicBlock(test), Call: engineFun.AppendBasicBlock(call));
                }).ToArray();
                if (blocks.Length == 0)
                {
                    var block = engineFun.AppendBasicBlock("main");
                    Current.Builder.PositionAtEnd(block);
                    Current.Builder.BuildRetVoid();
                }
                else
                {
                    var exit = engineFun.AppendBasicBlock("exit");
                    Current.Builder.PositionAtEnd(blocks[0].Test);                                                                         // ::Text
                    var scriptField = Current.Builder.BuildStructGEP2(Current.Libs.ActorStruct, engineFun.FirstParam, 4);   // Field: Scripts
                    var scriptsPtr = Current.Builder.BuildLoad2(Current.Libs.ScriptHolder.Pointer, scriptField, "Scripts");  // Get Scripts[]

                    for (uint i = 0; i < scripts.Count; i++) 
                    {                                                                                                                      // ::Test
                        var @goto = Current.Builder.BuildGEP2(Current.Libs.ScriptHolder, scriptsPtr, stackalloc[]               // Field: Scripts[i].break_point
                                { LLVMValueRef.CreateConst(i), LLVMValueRef.CreateConst(0) }, $"Script[{i}]::GOTO");                     // ^^^
                        var gotoVal = Current.Builder.BuildLoad2(Current.Context.Int32Type, @goto);                                // GOTO = Get Scripts[i].break_point
                        var isRunning = Current.Builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, gotoVal,                    // Is script running
                            LLVMValueRef.CreateConst<uint>(0), $"Script[{i}]::Running");                                            // ^^ (GOTO > 0)
                        Current.Builder.BuildCondBr(isRunning, blocks[i].Call, i != scripts.Count - 1 ? blocks[i + 1].Test : exit);   // if running goto Call else Next Test
                        Current.Builder.PositionAtEnd(blocks[i].Call);                                                                     // ::Call
                        var scriptPtr = Current.Builder.BuildGEP2(Current.Libs.ScriptHolder, scriptsPtr, stackalloc[]           // Get Script[i]
                                { LLVMValueRef.CreateConst(i) }, $"Script[{i}]");                                                        // ^^^
                        var stackPtr = Current.Builder.BuildStructGEP2(Current.Libs.ScriptHolder, scriptPtr, 2);            // Field: Script.stack
                        var stack = Current.Builder.BuildLoad2(Current.Libs.PointerType, stackPtr, $"Script[{i}]::Stack");   // Get stack ptr
                        Current.Builder.BuildCall2(scripts[(int)i].Function, [engineFun.GetParam(0), stack, @goto]);                           // Invoke Script
                        Current.Builder.BuildBr(i != scripts.Count - 1 ? blocks[i + 1].Test : exit);                                           // Goto next Test
                        if(i != scripts.Count - 1) Current.Builder.PositionAtEnd(blocks[i + 1].Test);
                    }
                    Current.Builder.PositionAtEnd(exit);                                                                                   // Exit
                    Current.Builder.BuildRetVoid();                                                                                            // Return;
                }

                return new(engineFun, engineType, Current.Context.VoidType, [Current.Libs.ActorStruct.Pointer], false);
            }
        }

        Current.Scripts = scriptList.ToFrozenDictionary();
    }
}