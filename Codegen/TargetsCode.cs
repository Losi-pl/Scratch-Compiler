using System.Collections.Immutable;
using LLVMSharp.Interop;
using ScratchCompiler.Parser;
using ScratchCompiler.Parser.Entries;
// ReSharper disable UseCollectionExpression

namespace ScratchCompiler.Codegen;

internal class TargetsCode
{
    private static CodegenTools Tools => Generator.Current;
    
    public readonly ImmutableArray<ScriptData> Scripts;
    public readonly TargetEvents Events;
    public readonly EmbeddedFile[] Costumes;
    public readonly LLVMModuleRef Module;
    public readonly LLVMValueRef Name;
    public readonly Target Target;

    public readonly LLVMMethod ScriptUpdate;
    public readonly LLVMMethod RunOnStart;
    
    public TargetsCode(Target target)
    {
        Target = target;
        Module = Tools.Context.CreateModuleWithName($"{Tools.Module.SourceFileName}::{target.Name}");
        
        if (target is Actor)
        {
            Name = Module.BuildGlobalString(target.Name, $"{target.Name}::Name");
            Name.Linkage = LLVMLinkage.LLVMLinkOnceODRLinkage;
        }
        else
            Name = LLVMValueRef.CreateConstPointerNull(Tools.Libs.PointerType);
        
        ScriptUpdate = Module.CreateMethod(Tools.Context.VoidType, $"{target.Name}::Update", target is Actor ? [Tools.Libs.PointerType] : []);
        RunOnStart = Module.CreateMethod(Tools.Context.VoidType, $"{target.Name}::Run::OnStart", target is Actor ? [Tools.Libs.PointerType] : []);

        if (target is Actor)
        {
            ScriptUpdate.Function.GetParam(0).Name = "actor";
            RunOnStart.Function.GetParam(0).Name = "actor";
        }
        
        {
            List<ScriptData> scripts = [];
            
            var onStarts = 0u;
            var onClick = 0u;
            var onBackgroundChange = 0u;
            var onBroadcast = 0u;
            var onCloned = 0u;
            var onGraterThan = 0u;
            var onKeyPress = 0u;
            foreach (var script in target.Blocks)
            {
                if(script.Entry is Dormant)
                    continue;

                var name = $"{target.Name}::{script.Entry switch {
                    OnStart => $"OnStart_{onStarts++}",
                    OnKeyPress => $"OnKeyPress_{onKeyPress++}",
                    OnBroadcast => $"OnBroadcast_{onBroadcast++}",
                    OnActorClick => $"OnClick_{onClick++}",
                    OnStageClick => $"OnClick_{onClick++}",
                    OnBackgroundChangedTo => $"OnBackgroundChange_{onBackgroundChange++}",
                    OnCloneCreated => $"OnCloned_{onCloned++}",
                    OnGraterThan => $"OnGraterThan_{onGraterThan++}",
                    _ => throw Generator.NotImplemented(script.Entry)
                }}";
                
                var scriptFun = Module.AddFunction(name, GenScript.ScriptBodyType);
                scripts.Add(new(script, new(scriptFun, GenScript.ScriptBodyType, Tools.Context.VoidType, 
                    [..GenScript.ScriptBodyArgs], false), false, 0));
                
                scriptFun.GetParam(0).Name = "actor";
                scriptFun.GetParam(1).Name = "scripts_stack";
                scriptFun.GetParam(2).Name = "goto_point";
            }

            Scripts = scripts.ToImmutableArray();
        }

        {
            GenScriptsUpdate(ScriptUpdate);
            GenStartForOnStart(RunOnStart);
            var events = GenEventsStruct(ScriptUpdate, RunOnStart);
            events.Linkage = LLVMLinkage.LLVMLinkOnceODRLinkage;
            Events = new(ScriptUpdate, RunOnStart, events);
        }

        Costumes = Generator.EmbedCostumes(target);
        
        foreach (var script in Scripts)
            GenScript.GenerateScript(this, script.Script, script.Function);
    }

    void GenScriptsUpdate(LLVMMethod method)
    {
        var fun = method.Function;
        fun.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeFunctionIndex, Tools.Libs.HotAttribute);
        var blocks = Scripts.Select((m, i) =>
        {
            var name = m.Function.Function.Name;
            name = name[(name.IndexOf(':') + 2)..];
            var test = i == 0 ? "entry" : name + "::Test";
            var call = name + "::Call";
            return (Test: fun.AppendBasicBlock(test), Call: fun.AppendBasicBlock(call));
        }).ToArray();
        if (blocks.Length == 0)
        {
            var block = fun.AppendBasicBlock("main");
            Tools.Builder.PositionAtEnd(block);
            Tools.Builder.BuildRetVoid();
        }
        else
        {
            LLVMValueRef actorPtr, scriptsPtr;
            var exit = fun.AppendBasicBlock("exit");
            Tools.Builder.PositionAtEnd(blocks[0].Test); if (Target is Actor) {                                                 // ::Text
                var scriptField = Tools.Builder.BuildStructGEP2(Tools.Libs.ActorStruct, fun.FirstParam, 4);      // Field: Scripts
                scriptsPtr = Tools.Builder.BuildLoad2(Tools.Libs.ScriptHolder.Pointer, scriptField, "Scripts");               // Get Scripts[]
                actorPtr = fun.GetParam(0);
            }
            else
            {
                actorPtr = LLVMValueRef.CreateConstPointerNull(Tools.Libs.PointerType); //TODO: Those are not valid operations
                scriptsPtr = actorPtr;
            }
            
            for (uint i = 0; i < Scripts.Length; i++) {                                                                         // >> ::Test
                var @goto = Tools.Builder.BuildGEP2(Tools.Libs.ScriptHolder, scriptsPtr, stackalloc[]                // Field: Scripts[i].break_point
                        { LLVMValueRef.CreateConst(i), LLVMValueRef.CreateConst(0) }, $"Script[{i}]::GOTO");                  // ^^^
                var gotoVal = Tools.Builder.BuildLoad2(Tools.Context.Int32Type, @goto);                                 // GOTO = Get Scripts[i].break_point
                var isRunning = Tools.Builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, gotoVal,                   // Is script running
                    LLVMValueRef.CreateConst<uint>(0), $"Script[{i}]::Running");                                         // ^^ (GOTO > 0)
                Tools.Builder.BuildCondBr(isRunning, blocks[i].Call, i != Scripts.Length - 1 ? blocks[i + 1].Test : exit); // if running goto Call else Next Test
                Tools.Builder.PositionAtEnd(blocks[i].Call);                                                                    // ::Call
                var scriptPtr = Tools.Builder.BuildGEP2(Tools.Libs.ScriptHolder, scriptsPtr, stackalloc[]            // Get Script[i]
                        { LLVMValueRef.CreateConst(i) }, $"Script[{i}]");                                                     // ^^^
                var stackPtr = Tools.Builder.BuildStructGEP2(Tools.Libs.ScriptHolder, scriptPtr, 2);             // Field: Script.stack
                var stack = Tools.Builder.BuildLoad2(Tools.Libs.PointerType, stackPtr, $"Script[{i}]::Stack");    // Get stack ptr
                Tools.Builder.BuildCall2(Scripts[(int)i].Function, [actorPtr, stack, @goto]);                                // Invoke Script
                Tools.Builder.BuildBr(i != Scripts.Length - 1 ? blocks[i + 1].Test : exit);                                         // Goto next Test
                if (i != Scripts.Length - 1) Tools.Builder.PositionAtEnd(blocks[i + 1].Test);
            }
            Tools.Builder.PositionAtEnd(exit);                                                                                  // Exit
            Tools.Builder.BuildRetVoid();                                                                                           // Return;
        }
    }

    void GenStartForOnStart(LLVMMethod method)
    {
        var fun = method.Function;
        Tools.Builder.PositionAtEnd(fun.AppendBasicBlock("main"));

        if (Target is Actor)
        {
            var scriptsPtr = Tools.Builder.BuildStructGEP2(Tools.Libs.ActorStruct, fun.GetParam(0), 4, "field:Scripts");
            var scriptsVal = Tools.Builder.BuildLoad2(Tools.Libs.ScriptHolder.Pointer, scriptsPtr, "Scripts");

            for (int i = 0; i < Scripts.Length; i++)
                if (Scripts[i].Script.Entry is OnStart)
                    Tools.Builder.BuildCall2(Tools.Libs.RunScript, [scriptsVal, LLVMValueRef.CreateConst((uint)i)]);
        }
        
        Tools.Builder.BuildRetVoid();
    }

    LLVMValueRef GenEventsStruct(LLVMMethod update, LLVMMethod onStart)
    {
        unsafe
        {
            fixed (LLVMValueRef* fields = stackalloc LLVMValueRef[2])
            {
                fields[0] = update.Function;
                fields[1] = onStart.Function;
                        
                var structVal = LLVM.ConstNamedStruct(Tools.Libs.ScriptEvents.Type, (LLVMOpaqueValue**)fields, 2);
                using var name = new MarshaledString($"{Target.Name}::Events");
                var global = LLVM.AddGlobal(Module, Tools.Libs.ScriptEvents.Type, name);
                LLVM.SetInitializer(global, structVal);
                        
                LLVM.SetLinkage(global, LLVMLinkage.LLVMLinkerPrivateLinkage);           // private
                LLVM.SetUnnamedAddr(global, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr); // unnamed_addr
                LLVM.SetGlobalConstant(global, 1 /* true */);                   // constant
                LLVM.SetAlignment(global, 1);                                      // align 1

                return global;
            }
        }
    }
}