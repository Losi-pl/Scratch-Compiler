using System.Collections.Frozen;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Decoder;

public class Project
{
    [ThreadStatic]
    internal static (Project project, ZipArchive archive) CurrentProject;
    
    private Project(JsonObject json, ZipArchive archive)
    {
        CurrentProject = (this, archive);
        {
            var meta = json["meta"] as JsonObject ?? throw new Exception();
            FileFormat = Version.TryParse(meta["semver"]?.GetValue<string>() ?? string.Empty, out var version) ? version : throw new Exception();
            VirtualMachine = Version.TryParse(meta["vm"]?.GetValue<string>() ?? string.Empty, out version) ? version : throw new Exception();
        }
        switch (FileFormat)
        {
            case { Major: 3, Minor: 0, Build: 0 }:
            {
                HashSet<Actor> actors = new();
                foreach (var targetNode in json["targets"]?.AsArray() ?? throw new Exception())
                {
                    var target = (JsonObject)targetNode!;
                    if ((target["isStage"]?.GetValue<bool>() ?? true)!)
                        Scene = new Scene(target);
                    else
                        actors.Add(new Actor(target));
                }

                Actors = actors.ToFrozenSet();
                
                Scene!.LoadCode();
                foreach (var target in Actors)
                    target.LoadCode();
                
            } break;
        }

        CurrentProject = (null!, null!);
    }
    
    
    
    public static Project Load(Stream source)
    {
        var archive = ZipArchive.CreateAsync(source, ZipArchiveMode.Read, true, Encoding.Default).Result;
        
        return new(JsonNode.Parse(
            archive.GetEntry("project.json")?.Open() ?? 
                throw new InvalidOperationException("Invalid target file")) as JsonObject ?? 
                throw new InvalidOperationException("Invalid target file"),
            archive);
    }
    
    public Version FileFormat { get; }
    public Version VirtualMachine { get; }
    public FrozenSet<Actor> Actors { get; }
    public Scene Scene { get; }

    public AllBroadcasts Broadcasts => new(this);
}