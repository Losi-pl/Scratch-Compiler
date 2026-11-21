using LLVMSharp.Interop;

namespace ScratchCompiler.Codegen;

public class CodeSet
{
    private readonly LLVMModuleRef _system;
    private readonly LLVMModuleRef _resources;
    private readonly LLVMModuleRef[] _targets;

    internal CodeSet(LLVMModuleRef system, LLVMModuleRef resources, LLVMModuleRef[] targets)
    {
        _system = system;
        _resources = resources;
        _targets = targets;
    }
    
    public void Compile(string path)
    {
        var triple = LLVMTargetRef.DefaultTriple;
        
        // Create a target machine and emit an object file
        var target = LLVMTargetRef.GetTargetFromTriple(triple);
        var targetMachine = target.CreateTargetMachine(
            triple, "generic", "",
            LLVMCodeGenOptLevel.LLVMCodeGenLevelNone,
            LLVMRelocMode.LLVMRelocDefault,
            LLVMCodeModel.LLVMCodeModelDefault
        );

        var tempFolder = Path.GetTempPath() + Path.GetFileNameWithoutExtension(Path.GetTempFileName());
        Directory.CreateDirectory(tempFolder);

        var objects = new List<string>();
        foreach (var (module, name) in new[] { (_system, "scratch"), (_resources, "resources") }.Concat(_targets.Select(a => (a, Guid.CreateVersion7().ToString()))))
        {
            var filePath = $"{tempFolder}\\{name}.obj";
            objects.Add(filePath); File.Create(filePath).Close();
            Console.WriteLine($"Compiling: {module.SourceFileName}");
            targetMachine.EmitToFile(module, filePath, LLVMCodeGenFileType.LLVMObjectFile);
        }

        #if DEBUG
        var sysroot = Environment.GetEnvironmentVariable("llvm-sysroot");
        #else
        var sysroot = Path.Combine(AppContext.BaseDirectory, "libs/");
        #endif

        var linkArgs = objects.Concat([
            // ReSharper disable UseCollectionExpression, StringLiteralTypo
            $"/out:{path}",
            Path.Combine(AppContext.BaseDirectory, "libScratch.lib"), 
            // C++ Runtime Libs
            "/defaultlib:libcmt.lib", 
            "/defaultlib:libvcruntime.lib",
            "/defaultlib:libcpmt.lib",
            // System Libs
            "user32.lib", 
            "gdi32.lib", 
            "opengl32.lib", 
            "winmm.lib",
            "advapi32.lib",
            "shell32.lib",
            "ole32.lib",
            // Sysroot
            $"/libpath:{sysroot}/crt/lib/x86_64",
            $"/libpath:{sysroot}/sdk/lib/um/x86_64",
            $"/libpath:{sysroot}/sdk/lib/ucrt/x86_64",
            // Attributes
            "/noimplib"
            // ReSharper restore UseCollectionExpression, StringLiteralTypo
        ]);
        
        using var linkProcess = System.Diagnostics.Process.Start("lld-link", linkArgs);
        linkProcess.WaitForExit();
        
        Directory.Delete(tempFolder, true);
    }

    public void Dump()
    {
        foreach (var module in new[]{_system}.Concat(_targets))
            module.Dump();
    }
}