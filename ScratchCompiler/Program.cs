using ScratchCompiler.Parser;

if (args.Length == 0)
    Console.WriteLine("Provide a fille to be compiled");

var project = Project.Load(File.OpenRead(Path.GetFullPath(args[0])));


for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-m" or "--manifest":
        {
            Console.WriteLine("Scene:");
            Console.WriteLine($"    Backgrounds: {project.Scene.Costumes.Length}");
            foreach (var scene in project.Scene.Costumes)
                Console.WriteLine($"     - {scene.Name}");
            Console.WriteLine($"    Scripts: {project.Scene.Blocks.Count}");
            Console.WriteLine($"    Variables: {project.Scene.Variables.Length}");
            foreach (var variable in project.Scene.Variables)
                Console.WriteLine($"     - {variable.Name}");

            foreach (var actor in project.Actors)
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
            
        } return;

        case "-s" or "--scripts":
        {
            IEnumerable<Target> sources;
            if (HasArg(1))
            {
                var name = Arg(1);
                var target = project.Scene.Name == name
                    ? (Target)project.Scene
                    : project.Actors.FirstOrDefault(a => a.Name == name);
                if (target is null)
                {
                    Console.WriteLine($"Actor {name} not found");
                    return;
                }

                sources = Enumerable.Repeat(target, 1);
            }
            else
                sources = Enumerable.Repeat<Target>(project.Scene, 1).Concat(project.Actors);

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
            
        } return;
    }

    bool HasArg(int index) => args.Length > i + index;
    string Arg(int index) => args[i + index];
}