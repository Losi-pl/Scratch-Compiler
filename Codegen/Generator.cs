#pragma warning disable CS8618
using System.Collections.Frozen;
using Humanizer;
using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

public static partial class Generator
{
    [ThreadStatic] internal static CodegenTools Current;
    
    public static CodeSet GenerateCode(this SyntaxTree tree, string? projectName = null)
    {
        var context = LLVMContextRef.Create();
        var module = context.CreateModuleWithName(projectName ?? "scratch-app");
        var builder = context.CreateBuilder();

        Current = new()
        {
            Context = context,
            Module = module,
            Builder = builder,
            ResourceModule = context.CreateModuleWithName($"{module.SourceFileName}::Resources")
        };
        Current.Libs = new(Current);
        
        var main = LLVMMethod.CreateMethod(context.VoidType, "main", []);
        var actorEnumeration = GenActorEnumerationBody(builder);
        var scriptUpdates = GenRunScriptUpdates();
        var runOnStartScripts = GenRunOnStartScripts();

        Current.Targets = tree.GetAllTargets().Select(t => new KeyValuePair<Target, TargetsCode>(t, new(t))).ToFrozenDictionary();
        foreach (var target in Current.Targets.Values)
            target.GenMethodContent();
        
        // Build the body of `main`
        {
            
            var mainBlock = main.Function.AppendBasicBlock("main");
            builder.PositionAtEnd(mainBlock);
            
            builder.BuildCall2(Current.Libs.ScratchInit, []);
            builder.BuildCall2(Current.Libs.InitWindow, [builder.BuildGlobalString(module.SourceFileName, "Scratch::Project-Name")]);
            foreach (var target in tree.GetAllTargets())
                builder.BuildCall2(Current.Targets[target].ConstructTarget, []);
            builder.BuildCall2(actorEnumeration, [runOnStartScripts.Function, Current.Libs.Nullptr]);
            builder.BuildCall2(actorEnumeration, [scriptUpdates.Function, Current.Libs.Nullptr]);
            builder.BuildCall2(Current.Libs.RunApp, []);

            builder.BuildCall2(Current.Libs.ExitApp, [LLVMValueRef.CreateConst(0u)]);
            builder.BuildUnreachable();
        }
        
        LLVM.InitializeNativeTarget();
        LLVM.InitializeNativeAsmParser();
        LLVM.InitializeNativeAsmPrinter();

        Current.Module.SourceFileName += "::Main";
        return new(Current.Module, Current.ResourceModule, tree.GetAllTargets().Select(t => Current.Targets[t].Module).ToArray());
    }

    internal static Exception NotImplemented(object element) => (Exception)System.Reflection.Assembly.GetAssembly(typeof(System.Reflection.Assembly))!
        .GetType("System.NotImplementedException")!.GetConstructors()[1].Invoke([$"Element {element.ToString().Truncate(32)} is not yet implemented"]);
}