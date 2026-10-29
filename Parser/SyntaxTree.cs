using System.Collections;
using System.Collections.Frozen;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser;

public class SyntaxTree
{
    [ThreadStatic]
    internal static (SyntaxTree project, ZipArchive archive) Current;
    
    private SyntaxTree(JsonObject json, ZipArchive archive)
    {
        Current = (this, archive);
        {
            var meta = json["meta"] as JsonObject ?? throw new Exception();
            FileFormat = Version.TryParse(meta["semver"]?.GetValue<string>() ?? string.Empty, out var version) ? version : throw new Exception();
            VirtualMachine = Version.TryParse(meta["vm"]?.GetValue<string>() ?? string.Empty, out version) ? version : throw new Exception();
        }
        switch (FileFormat)
        {
            case { Major: 3, Minor: 0, Build: _, Revision: _ }:
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

        Current = (null!, null!);
    }
    
    
    
    public static SyntaxTree Load(Stream source)
    {
        var archive = ZipArchive.CreateAsync(source, ZipArchiveMode.Read, true, Encoding.Default).Result;
        
        return new(JsonNode.Parse(
            archive.GetEntry("project.json")?.Open() ?? 
                throw new InvalidOperationException("Invalid target file")) as JsonObject ?? 
                throw new InvalidOperationException("Invalid target file"),
            archive);
    }

    public EnumerateTargets GetAllTargets() => new(this);
    
    public Version FileFormat { get; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Version VirtualMachine { get; }
    public FrozenSet<Actor> Actors { get; }
    public Scene Scene { get; }

    public AllBroadcasts Broadcasts => new(this);
    public AllVariables Variables => new(this);
    
    public readonly struct EnumerateTargets(SyntaxTree tree) : IEnumerable<Target>
    {
        public Enumerator GetEnumerator() => new(tree);
        
        IEnumerator<Target> IEnumerable<Target>.GetEnumerator() => new Enumerator(tree);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(tree);
        
        public struct Enumerator(SyntaxTree tree) : IEnumerator<Target>
        {
            private Target? _scene = tree.Scene;
            private bool _ = true;
            FrozenSet<Actor>.Enumerator _actors = tree.Actors.GetEnumerator();
            
            public bool MoveNext()
            {
                if (_)
                {
                    // ReSharper disable once AssignmentInsteadOfDiscard
                    _ = false;
                    return true;
                }

                if (_scene is not null)
                    _scene = null;
                return _actors.MoveNext();
            }

            public void Reset() => throw new NotSupportedException();

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public Target Current => _scene ?? _actors.Current;

            object IEnumerator.Current => _scene ?? _actors.Current;

            public void Dispose() { }
        }
    }
}