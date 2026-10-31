using LLVMSharp.Interop;
using ScratchCompiler.Codegen.ValueTypes;

namespace ScratchCompiler.Codegen;

internal static class Helpers
{
    public static LLVMValueRef BuildCall2(this in LLVMBuilderRef builder, LLVMMethod method, LLVMValueRef[] args, string name = "")
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(name);
        
        return builder.BuildCall2(method.FunctionType, method.Function, args, name);
    }
    
    public static LLVMValueRef CreateGlobalConstString(this in LLVMBuilderRef builder, string value, string name = "")
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(name);
        
        unsafe
        {
            using var text = new MarshaledString(value);
            
            var contentTy  = LLVMTypeRef.CreateArray(Generator.Current.Context.Int8Type, (uint)(text.Length + 1));
            var contentCst = LLVM.ConstStringInContext2(Generator.Current.Context, text, (uint)text.Length, 0);

            var @const = Generator.Current.Module.AddGlobal(contentTy, name);
            LLVM.SetInitializer(@const, contentCst);
            LLVM.SetGlobalConstant(@const, 1);
            LLVM.SetLinkage(@const, LLVMLinkage.LLVMPrivateLinkage);
            LLVM.SetUnnamedAddr(@const, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr);
            LLVM.SetAlignment(@const, 1);
            return @const;
        }
    }

    extension(LLVMTypeRef typeRef)
    {
        public static LLVMTypeRef CreateStruct(ReadOnlySpan<LLVMTypeRef> fields, bool packed, string name)
        {
            if (string.IsNullOrEmpty(name))
                return LLVMTypeRef.CreateStruct(fields, packed);
            else unsafe
            {
                using var n = new MarshaledString(name);
                var strType = LLVM.StructCreateNamed(Generator.Current.Context, n);
                if (fields.IsEmpty) return strType;
                
                LLVMOpaqueType** fieldsPtr = stackalloc LLVMOpaqueType*[fields.Length];
                for (var i = 0; i < fields.Length; i++)
                    fieldsPtr[i] = fields[i];

                LLVM.StructSetBody(strType, fieldsPtr, (uint)fields.Length, packed ? 1 : 0);

                return strType;
            }
        }
    }

    extension(LLVMValueRef valueRef)
    {
        public static LLVMValueRef CreateConst<T>(T value) where T : struct
        {
            return value switch
            {
                bool b => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int1Type, b ? 1u : 0),
                byte i8 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int8Type, i8),
                char i8 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int8Type, i8),
                ushort i16 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int16Type, i16),
                uint i32 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int32Type, i32),
                ulong i64 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int64Type, i64),
                float f => LLVMValueRef.CreateConstReal(Generator.Current.Context.FloatType, f),
                double d => LLVMValueRef.CreateConstReal(Generator.Current.Context.DoubleType, d),
                sbyte i8 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int8Type, (ulong)i8),
                short i16 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int16Type, (ulong)i16),
                int i32 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int32Type, (ulong)i32),
                long i64 => LLVMValueRef.CreateConstInt(Generator.Current.Context.Int64Type, (ulong)i64),
                _ => throw new NotSupportedException($"Value of type {value.GetType()} is not supported")
            };
        }
    }
    
    public static void BuildPrintLine(this LLVMBuilderRef builder, CharPointer text, string name = "") =>
        builder.BuildCall2(Generator.Current.Libs.Puts, [text], name);
    public static CharPointer BuildStrCombine(this LLVMBuilderRef builder, CharPointer left, CharPointer right, string name = "") =>
        builder.BuildCall2(Generator.Current.Libs.CombineStrings2, [left, right], name);
    public static CharPointer BuildStrCombine(this LLVMBuilderRef builder, CharPointer left, CharPointer center, CharPointer right, string name = "") =>
        builder.BuildCall2(Generator.Current.Libs.CombineStrings3, [left, center, right], name);
}