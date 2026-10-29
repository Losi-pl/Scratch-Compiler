using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen.ValueTypes;

public struct VoidPointer : IEquatable<CharPointer>
{
    public LLVMValueRef Value;
    
    public bool Equals(CharPointer other) => Value == other.Value;

    public static implicit operator VoidPointer(CharPointer pointer) => new() { Value = pointer.Value };
    public static explicit operator CharPointer(VoidPointer pointer) => new() { Value = pointer.Value };
    public static implicit operator LLVMValueRef(VoidPointer pointer) => pointer.Value;
    public static implicit operator VoidPointer(LLVMValueRef value) => new() { Value = value };
}