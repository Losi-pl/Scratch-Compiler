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

    private StructType(LLVMTypeRef type, LLVMTypeRef pointer, ImmutableArray<LLVMTypeRef> fields, bool isPacked, string? name, ulong byteSize)
    { Type = type; Pointer = pointer; Fields = fields; IsPacked = isPacked; Name = name; ByteSize = byteSize; }
    
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
    
    public static StructType CreateListHandler()
    {
        var fullName = "std::list";
        LLVMTypeRef listNode;
        {
            using var name = new MarshaledString(fullName);
            unsafe { listNode = LLVM.StructCreateNamed(Generator.Current.Context, name); }
        }

        var fields = new LLVMTypeRef[3];
        fields[0] = LLVMTypeRef.CreatePointer(listNode, 0);
        fields[1] = fields[0];
        fields[2] = Generator.Current.Context.CreatePointerType(0);

        unsafe
        {
            LLVMOpaqueType** fieldsPtr = stackalloc LLVMOpaqueType*[fields.Length];
            for (var i = 0; i < fields.Length; i++)
                fieldsPtr[i] = fields[i];

            LLVM.StructSetBody(listNode, fieldsPtr, (uint)fields.Length, 0);
        }
        
        unsafe
        {
            LLVMTargetDataRef targetData = LLVM.GetModuleDataLayout(Generator.Current.Module);
            var byteSize = LLVM.ABISizeOfType(targetData, listNode);
            // ReSharper disable once UseCollectionExpression
            return new StructType(listNode, LLVMTypeRef.CreatePointer(listNode, 0), fields.ToImmutableArray(), false, fullName, byteSize);
        }
    }
    
    public static implicit operator LLVMTypeRef(StructType type) => type.Type;
}