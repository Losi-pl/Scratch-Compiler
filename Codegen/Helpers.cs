using LLVMSharp.Interop;

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
            fixed (char* text = value)
            {
                var contentTy  = LLVMTypeRef.CreateArray(Generator.Current.Context.Int8Type, 2);
                var contentCst = LLVM.ConstStringInContext2(Generator.Current.Context, (sbyte*)text, 2, 0);

                var @const = Generator.Current.Module.AddGlobal(contentTy, name);
                LLVM.SetInitializer(@const, contentCst);
                LLVM.SetGlobalConstant(@const, 1);
                LLVM.SetLinkage(@const, LLVMLinkage.LLVMPrivateLinkage);
                LLVM.SetUnnamedAddr(@const, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr);
                LLVM.SetAlignment(@const, 1);
                return @const;
            }
        }
    }
}