using System.Collections.Immutable;
using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen.ValueTypes;

public struct StructType
{
    public readonly LLVMTypeRef Type;
    public readonly LLVMTypeRef Pointer;
    public readonly ImmutableArray<LLVMTypeRef> Fields;
    public readonly bool IsPacked;
    public readonly string? Name;
    public readonly ulong ByteSize;
    
    public static StructType Create(ReadOnlySpan<LLVMTypeRef> fields, bool packed, string name = "") => new(fields, packed, name);
    
    private StructType(ReadOnlySpan<LLVMTypeRef> fields, bool packed, string name)
    {
        Type = LLVMTypeRef.CreateStruct(fields, packed, name);
        Pointer = LLVMTypeRef.CreatePointer(Type, 0);
        // ReSharper disable once UseCollectionExpression
        Fields = fields.ToImmutableArray();
        IsPacked = packed;
        Name = string.IsNullOrEmpty(name) ? null : name;
        unsafe
        {
            LLVMTargetDataRef targetData = LLVM.GetModuleDataLayout(Generator.Current.Module);
            ByteSize = LLVM.ABISizeOfType(targetData, Type);
        }
    }
    public static implicit operator LLVMTypeRef(StructType type) => type.Type;
}