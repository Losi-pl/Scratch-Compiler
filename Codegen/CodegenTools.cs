using System.Collections.Frozen;
using System.Collections.Immutable;
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

    public FrozenDictionary<Target, ((CodeBlock script, LLVMMethod method, bool defined, uint stackSize)[] scripts, LLVMMethod engine)> Scripts;

    public FrozenDictionary<Target, CharPointer> TargetNames = null!;
}