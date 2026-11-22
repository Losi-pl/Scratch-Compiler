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
    public readonly ImmutableArray<EmbeddedFile> Costumes;
    public readonly LLVMModuleRef Module;
    public readonly LLVMValueRef Name;
    public readonly Target Target;

    public readonly OneOf.OneOf<LLVMValueRef, (
        LLVMValueRef Events, 
        (LLVMValueRef Body, LLVMValueRef Count) Scripts, 
        (LLVMValueRef Body, LLVMValueRef Index) Backdrops)> 
    InstanceData;
    
    public readonly LLVMMethod ConstructTarget;
    public readonly LLVMMethod ScriptUpdate;
    public readonly LLVMMethod RunOnStart;
    
    public TargetsCode(Target target)
    {
        Target = target;
        Module = Tools.Context.CreateModuleWithName(target is Actor ? $"{Tools.Module.SourceFileName}::Actor::{target.Name}" : $"{Tools.Module.SourceFileName}::Stage");
        Name = target is Actor ? Module.BuildGlobalString(target.Name, $"{target.Name}::Name") : LLVMValueRef.CreateConstPointerNull(Tools.Libs.PointerType);
        ConstructTarget = Module.CreateMethod(Tools.Context.VoidType, target is Actor ? $"Construct::Actor::{target.Name}" : "Construct::Stage" , []);
        ScriptUpdate = Module.CreateMethod(Tools.Context.VoidType, $"{target.Name}::Update", target is Actor ? [Tools.Libs.PointerType] : []);
        RunOnStart = Module.CreateMethod(Tools.Context.VoidType, $"{target.Name}::Run::OnStart", target is Actor ? [Tools.Libs.PointerType] : []);

        if (target is Actor) 
        {
            ScriptUpdate.Function.GetParam(0).Name = "actor";
            RunOnStart.Function.GetParam(0).Name = "actor";

            var @static = Module.AddGlobal(Tools.Libs.ActorStruct, $"Actor::Main::{target.Name}");
            @static.Initializer = LLVMValueRef.CreateConstNull(Tools.Libs.ActorStruct);
            @static.Alignment = 8;
            InstanceData = @static;
        }
        else
        {
            var events = Module.AddGlobal(Tools.Libs.ScriptEvents.Pointer, "stage_events");
            var scripts = Module.AddGlobal(Tools.Libs.PointerType, "stage_scripts");
            var scriptsCount = Module.AddGlobal(Tools.Context.Int32Type, "stage_script_count");
            var backdrop = Module.AddGlobal(Tools.Libs.PointerType, "stage_backdrops");
            var backdropCount = Module.AddGlobal(Tools.Context.Int32Type, "stage_backdrop_index");
            InstanceData = (events, (scripts, scriptsCount), (backdrop, backdropCount));
        }
        
        if(true) {
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
                    OnCloneCreated => $"OnCloned_{onCloned++}",
                    OnBroadcast => $"OnBroadcast_{onBroadcast++}",
                    OnGraterThan => $"OnGraterThan_{onGraterThan++}",
                    OnActorClick or OnStageClick => $"OnClick_{onClick++}",
                    OnBackgroundChangedTo => $"OnBackgroundChange_{onBackgroundChange++}",
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
            Events = new(ScriptUpdate, RunOnStart, events);
        }

        Costumes = Generator.EmbedCostumes(target);
    }

    public void GenMethodContent()
    {
        foreach (var script in Scripts)
            script.StackSize = GenScript.GenerateScript(this, script.Script, script.Function);
        
        GenTargetConstructor(ConstructTarget);
    }
    
    void GenTargetConstructor(LLVMMethod method)
    {
        var builder = Tools.Builder;
        builder.PositionAtEnd(method.Function.AppendBasicBlock("main"));
        if (Target is Actor actor) 
        {
            var posX = LLVMValueRef.CreateConst(actor.DefaultPosition.x);                                        // Const: Start position x
            var posY = LLVMValueRef.CreateConst(actor.DefaultPosition.y);                                        // Const: Start position y
            var scriptCount = LLVMValueRef.CreateConst(Scripts.Length);                                          // Const: Count of Actor scripts
            var defCostume = LLVMValueRef.CreateConst(actor.DefaultCostume);                                     // Const: Default Costume index
            var costumeCount = LLVMValueRef.CreateConst((uint)actor.Costumes.Length);                            // Const: Costume count
            var actorScale = LLVMValueRef.CreateConst(actor.DefaultSize);                                        // Const: Actor size

            var inst = InstanceData.AsT0;
            var costumes = builder.BuildCall2(Tools.Libs.CreateCostumeStorage, [costumeCount],       // Construct costume storage
                "costume_storage");                                                                         // ^^^
            builder.BuildCall2(Tools.Libs.ConstructActor, [inst, Name,                                           // Construct actor's body
                Events.Struct, posX, posY, scriptCount, costumes, defCostume, actorScale]);                      // ^^^
            builder.BuildCall2(Tools.Libs.RegisterActor, [inst]);                                                // Register the actor
            if (Scripts.Length > 0)
            {
                var scriptField = builder.BuildStructGEP2(Tools.Libs.ActorStruct, inst, 4,    // Find field of script array
                    "field:scripts");                                                                      // ^^^
                var scriptArray = builder.BuildLoad2(Tools.Libs.ScriptHolder.Pointer,                // Get script list
                    scriptField, "scripts");                                                               // ^^^
                for (int i = 0; i < Scripts.Length; i++)
                {
                    var scriptPtr = builder.BuildGEP2(Tools.Libs.ScriptHolder, scriptArray,       // scripts[i]
                        new[] { LLVMValueRef.CreateConst(i) }, $"script[{i}]");                     // ^^^
                    builder.BuildCall2(Tools.Libs.ConstructScript,                                               // Construct script handler
                        [scriptPtr, LLVMValueRef.CreateConst(Scripts[i].StackSize)]);                            // ^^^
                }
            }
            
            foreach (var costume in actor.Costumes.Zip(Costumes).Select((c, i) => (sys: c.First, llvm: c.Second, ind: i)))
            {
                var index = LLVMValueRef.CreateConst((uint)costume.ind);
                var name = builder.BuildGlobalString(costume.sys.Name, $"{actor.Name}::{costume.sys.AssetID}::Name");
                if (costume.sys.FileFormat == CostumeFormat.SVG)
                    builder.BuildCall2(Tools.Libs.LoadCostumeFromSvg, [costumes, index, costume.llvm.Content, name]);
                else
                {
                    var size = builder.BuildLoad2(Tools.Context.Int64Type, costume.llvm.Size!.Value, $"costume_{costume.ind}_size");
                    builder.BuildCall2(Tools.Libs.LoadCostumeFromData, [costumes, index, costume.llvm.Content, size, name]);
                }
            }

        }
        else 
        {
            builder.BuildStore(Events.Struct, InstanceData.AsT1.Events);
            
            var scriptArray = builder.BuildCall2(Tools.Libs.AlignedMalloc, [
                LLVMValueRef.CreateConst(Tools.Libs.ScriptHolder.ByteSize * (uint)Target.Blocks.Count),
                LLVMValueRef.CreateConst((ulong)8), // align 8
            ], "scripts");
            builder.BuildStore(scriptArray, InstanceData.AsT1.Scripts.Body);                             // Save scripts
            builder.BuildStore(LLVMValueRef.CreateConst((uint)Target.Blocks.Count),                          // Save scripts count
                InstanceData.AsT1.Scripts.Count);                                                            // ^^^
            for (int i = 0; i < Scripts.Length; i++)
            {
                var scriptPtr = builder.BuildGEP2(Tools.Libs.ScriptHolder, scriptArray,           // scripts[i]
                    new[] { LLVMValueRef.CreateConst(i) }, $"script[{i}]");                         // ^^^
                builder.BuildCall2(Tools.Libs.ConstructScript,                                                   // Construct script handler
                    [scriptPtr, LLVMValueRef.CreateConst(Scripts[i].StackSize)]);                                // ^^^
            }
            
            var costumeCount = LLVMValueRef.CreateConst((uint)Target.Costumes.Length);                           // Const: Costume count
            var costumes = builder.BuildCall2(Tools.Libs.CreateCostumeStorage, [costumeCount],       // Construct costume storage
                "costume_storage");                                                                         // ^^^
            builder.BuildStore(costumes, InstanceData.AsT1.Backdrops.Body);                              // Save backdrop storage
            builder.BuildStore(LLVMValueRef.CreateConst(Target.DefaultCostume),                              // Save default backdrop index
                InstanceData.AsT1.Backdrops.Index);                                                          // ^^^
            foreach (var costume in Target.Costumes.Zip(Costumes).Select((c, i) => (sys: c.First, llvm: c.Second, ind: i)))
            {
                var index = LLVMValueRef.CreateConst((uint)costume.ind);                                         // Const: Index
                var name = builder.BuildGlobalString(costume.sys.Name,                               // Const: Name
                    $"{Target.Name}::{costume.sys.AssetID}::Name");                                        // ^^^
                if (costume.sys.FileFormat == CostumeFormat.SVG)
                    builder.BuildCall2(Tools.Libs.LoadCostumeFromSvg, [costumes, index, costume.llvm.Content, name]);
                else
                {
                    var size = builder.BuildLoad2(Tools.Context.Int64Type, costume.llvm.Size!.Value, $"costume_{costume.ind}_size");
                    builder.BuildCall2(Tools.Libs.LoadCostumeFromData, [costumes, index, costume.llvm.Content, size, name]);
                }
            }
        }
        
        builder.BuildRetVoid();
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
                scriptsPtr = Tools.Builder.BuildLoad2(Tools.Libs.ScriptHolder.Pointer, InstanceData.AsT1.Scripts.Body, "Scripts");
                actorPtr = LLVMValueRef.CreateConstPointerNull(Tools.Libs.PointerType);
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
                LLVM.SetAlignment(global, 8);                                      // align 8

                return global;
            }
        }
    }
}