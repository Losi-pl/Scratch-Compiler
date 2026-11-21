using System.Collections.Frozen;
using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

internal class CodegenTools
{
    public LLVMContextRef Context;
    public LLVMModuleRef Module;
    public LLVMModuleRef ResourceModule;
    public LLVMBuilderRef Builder;
    public LibraryParts Libs = null!;

    public FrozenDictionary<Target, TargetsCode> Targets = null!;
}