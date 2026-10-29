using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen.ValueTypes;

public struct CharPointer : IEquatable<CharPointer>
{
    public LLVMValueRef Value;
    
    public bool Equals(CharPointer other) => Value == other.Value;
    
    public static implicit operator LLVMValueRef(CharPointer pointer) => pointer.Value;
    public static implicit operator CharPointer(LLVMValueRef value) => new() { Value = value };
}