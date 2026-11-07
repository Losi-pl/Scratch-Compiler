using System.Collections.Frozen;
using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

internal class CodegenTools
{
    public LLVMContextRef Context;
    public LLVMModuleRef Module;
    public LLVMBuilderRef Builder;
    public LibraryParts Libs = null!;

    public FrozenDictionary<Target, (ScriptData[] Scripts, TargetEvents Events)> Scripts = null!;

    public FrozenDictionary<Target, CharPointer> TargetNames = null!;
}