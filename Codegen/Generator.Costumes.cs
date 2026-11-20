using LLVMSharp.Interop;
using ScratchCompiler.Parser;

namespace ScratchCompiler.Codegen;

partial class Generator
{
    internal static EmbeddedFile[] EmbedCostumes(Target target)
    {
        List<EmbeddedFile> costumes = [];
        var nullptr = LLVMValueRef.CreateConstPointerNull(Current.Libs.PointerType);
        foreach (var costume in target.Costumes)
        {
            if (costume.FileFormat == CostumeFormat.SVG)
            {
                using var stream = costume.OpenStream();
                using StreamReader reader = new(stream);
                var svg = reader.ReadToEnd();
                unsafe
                {
                    var bodyName = $"{target.Name}::Costume::{costume.AssetID}::SVG";
                    using var str = new MarshaledString(svg);
                    LLVMValueRef @const = LLVM.ConstStringInContext2(Current.Context, str, (nuint)str.Length, 0 /* false */);
                    var bodyType = LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, (uint)str.Length + 1);
                    var body = Current.ResourceModule.AddGlobal(bodyType, bodyName);
                    LLVM.SetInitializer(body, @const);
                    LLVM.SetLinkage(body, LLVMLinkage.LLVMLinkOnceODRLinkage);             // linkonce_odr
                    LLVM.SetUnnamedAddr(body, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr); // unnamed_addr
                    LLVM.SetGlobalConstant(body, 1 /* true */);                   // constant
                    LLVM.SetAlignment(body, 1);                                      // align 1
                    
                    costumes.Add(new(body, nullptr));
                }
            }
            else 
            {
                using var stream = costume.OpenStream();
                var memory = new MemoryStream();
                stream.CopyTo(memory);

                string bodyName = $"Actor::{target.Name}::Costume::{costume.AssetID}::Body";
                string sizeName = $"Actor::{target.Name}::Costume::{costume.AssetID}::Size";
                LLVMValueRef body, bodySize;
                unsafe {
                    LLVMValueRef @const;
                    fixed (byte* buffer = memory.ToArray())
                        @const = LLVM.ConstStringInContext2(Current.Context, (sbyte*)buffer, (nuint)memory.Length, 1 /* true */);
                    using var marshName = new MarshaledString(bodyName);
                    var bodyType = LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, (uint)memory.Length);
                    body = LLVM.AddGlobal(Current.ResourceModule, bodyType, marshName);
                    LLVM.SetInitializer(body, @const);
                    LLVM.SetLinkage(body, LLVMLinkage.LLVMLinkOnceODRLinkage);             // linkonce_odr
                    LLVM.SetUnnamedAddr(body, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr); // unnamed_addr
                    LLVM.SetGlobalConstant(body, 1 /* true */);                   // constant
                    LLVM.SetAlignment(body, 1);                                      // align 1
                    
                }

                unsafe {
                    bodySize = Current.ResourceModule.AddGlobal(Current.Context.Int64Type, sizeName);
                    var @const = LLVM.ConstInt(LLVMTypeRef.Int64, (ulong)memory.Length, 0);
                    LLVM.SetInitializer(bodySize, @const);
                    LLVM.SetLinkage(bodySize, LLVMLinkage.LLVMLinkOnceODRLinkage);             // linkonce_odr
                    LLVM.SetUnnamedAddr(bodySize, (int)LLVMUnnamedAddr.LLVMGlobalUnnamedAddr); // unnamed_addr
                    LLVM.SetGlobalConstant(bodySize, 1 /* true */);                   // constant
                    LLVM.SetAlignment(bodySize, 1);                                      // align 1
                }
                
                costumes.Add(new(body, bodySize));
            }
        }
        return costumes.ToArray();
    }
}