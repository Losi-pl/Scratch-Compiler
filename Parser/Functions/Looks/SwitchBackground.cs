using System.Text.Json.Nodes;
using OneOf;

namespace ScratchCompiler.Parser.Functions.Looks;

public class SwitchBackground: Function
{
    public const string ScratchCode = "looks_switchbackdropto";
    public const string ScratchNextCode = "looks_nextbackdrop";

    private readonly OneOf<Relative, Costume> _destination;
    
    public bool IsAbsolute => _destination.IsT1;
    public bool IsRelative => _destination.IsT0;

    public Relative AsRelative => _destination.AsT0;
    public Costume AsAbsolute => _destination.AsT1;
    
    private SwitchBackground(Relative direction) => _destination = direction;
    private SwitchBackground(Costume background) => _destination = background;
    
    public override string ToString() => _destination switch
    {
        { IsT0: true, AsT0: var t} => $"{t} background;",
        { IsT1: true, AsT1: var t } => $"Switch background to {{{t.Name}}};",
        _ => throw new InvalidOperationException()
    };

    [FunctionParser(ScratchNextCode)] // ReSharper disable once UnusedMember.Local
    private static SwitchBackground ParseNext(JsonObject _, JsonObject _1, Target _2) => new(Relative.Next);

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SwitchBackground Parse(JsonObject json, JsonObject list, Target _) {
        var @ref = json.GetInputReference("BACKDROP", list);
        var dest = @ref.GetField("BACKDROP");
        var background = SyntaxTree.Current.project.Scene.Costumes.FirstOrDefault(b => b.Name == dest);
        if (background is not null)
            return new SwitchBackground(background);
        return new SwitchBackground(dest switch
        {
            "next backdrop" => Relative.Next,
            "previous backdrop" => Relative.Previous,
            "random backdrop" => Relative.Random,
            _ => throw new NotSupportedException($"Unknown background {{{dest}}}")
        });
    }
    
    public enum Relative : byte
    {
        Next, Previous, Random
    }
}