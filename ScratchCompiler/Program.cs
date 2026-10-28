using ScratchCompiler.Parser;
using ScratchCompiler.Codegen;

if (args.Length == 0)
    Console.WriteLine("Provide a file to be compiled");

var projectPath = Path.GetFullPath(args[0]);
DisplayStage? displayStage = null;
string? name = null;
string? outPath = null;
var runAfter = false;

for (int i = 1; i < args.Length; ++i)
{
    switch (args[i])
    {
        case "-d" or "--display":
        {
            ++i;
            if (args.Length <= i)
            {
                Console.WriteLine("Argument \"display\" requires a stage specified [syntax | llvm]");
                return;
            }
            
            switch (args[i].ToLower())
            {
                case "syntax":
                    displayStage = DisplayStage.SyntaxTree;
                    break;
                case "llvm":
                    displayStage = DisplayStage.LLVM;
                    break;
            }
        } break;
        case "-o" or "--out":
        {
            ++i;
            if (args.Length <= i)
            {
                Console.WriteLine("Argument \"out\" requires a path specified");
                return;
            }
            
            outPath = Path.GetFullPath(args[i]);
        } break;
        case "-j" or "--jit":
            runAfter = true;
            break;
        default:
        {
            Console.WriteLine($"Unknown option \"{args[i]}\"");
            return;
        }
    }
}

var syntax = SyntaxTree.Load(File.OpenRead(projectPath));

if (displayStage is DisplayStage.SyntaxTree)
{
    Console.WriteLine("Scene:");
    Console.WriteLine($"    Backgrounds: {syntax.Scene.Costumes.Length}");
    foreach (var scene in syntax.Scene.Costumes)
        Console.WriteLine($"     - {scene.Name}");
    Console.WriteLine($"    Scripts: {syntax.Scene.Blocks.Count}");
    Console.WriteLine($"    Variables: {syntax.Scene.Variables.Length}");
    foreach (var variable in syntax.Scene.Variables)
        Console.WriteLine($"     - {variable.Name}");

    foreach (var actor in syntax.Actors)
    {
        Console.WriteLine($"\nActor {actor.Name}:");
        Console.WriteLine($"    Backgrounds: {actor.Costumes.Length}");
        foreach (var scene in actor.Costumes)
            Console.WriteLine($"     - {scene.Name}");
        Console.WriteLine($"    Scripts: {actor.Blocks.Count}");
        Console.WriteLine($"    Variables: {actor.Variables.Length}");
        foreach (var variable in actor.Variables)
            Console.WriteLine($"     - {variable.Name}");
    }
    
    Console.WriteLine("\n");
    
    var sources = Enumerable.Repeat<Target>(syntax.Scene, 1).Concat(syntax.Actors);
    var firstT = true;
    foreach(var target in sources)
    {
        Console.WriteLine($"{(firstT ? string.Empty : "\n")}{(target is Scene ? "Scene" : $"Actor [{target.Name}]")}:");
        var firstS = true;
        foreach (var codeBlock in target.Blocks)
        {
            var script = codeBlock.ToScript();
            script = script.Replace("\n", "\n    ");
            if(!firstS)
                Console.WriteLine('\n');
            Console.Write("    ");
            Console.WriteLine(script);
            firstS = false;
        }
        firstT = false;
    }
}

var code = syntax.GenerateCode(name);

if(displayStage is DisplayStage.LLVM)
    code.Dump();

outPath ??= Path.ChangeExtension(projectPath, ".exe");
var objectFile = code.Compile(outPath);

if(runAfter)
    code.Execute();

File.Delete(objectFile);

enum DisplayStage
{
    // ReSharper disable once InconsistentNaming
    SyntaxTree, LLVM
}