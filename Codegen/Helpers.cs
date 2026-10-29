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
    
    public static CharPointer BuildPrintLine(this LLVMBuilderRef builder, CharPointer text, string name = "") =>
        builder.BuildCall2(Generator.Current.Methods.Puts, [text], name);
    public static CharPointer BuildStrCombine(this LLVMBuilderRef builder, CharPointer left, CharPointer right, string name = "") =>
        builder.BuildCall2(Generator.Current.Methods.CombineStrings2, [left, right], name);
    public static CharPointer BuildStrCombine(this LLVMBuilderRef builder, CharPointer left, CharPointer center, CharPointer right, string name = "") =>
        builder.BuildCall2(Generator.Current.Methods.CombineStrings3, [left, center, right], name);
}