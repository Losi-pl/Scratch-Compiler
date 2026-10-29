using System.Collections.Frozen;
using System.Collections.Immutable;
using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

internal class CurrentTools
{
    public LLVMContextRef Context;
    public LLVMModuleRef Module;
    public LLVMBuilderRef Builder;
    public SystemMethods Methods = null!;

    public ImmutableArray<LLVMMethod> OnStartMethods;

    public FrozenDictionary<Target, LLVMValueRef> TargetNames = null!;
}