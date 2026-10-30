using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sound;

public class PlayUntilDone : Function
{
    public const string ScratchCode = "sound_playuntildone";
    
    public Parser.Sound Sound { get; }
    
    private PlayUntilDone(Parser.Sound sound) => Sound = sound;

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static PlayUntilDone Parse(JsonObject json, JsonObject list, Target target) {
        var @ref = json.GetInputReference("SOUND_MENU", list);
        var name = @ref.GetField("SOUND_MENU");
        var sound = target.Sounds.FirstOrDefault(c => c.Name == name) ?? throw new KeyNotFoundException(name);
        return new PlayUntilDone(sound);
    }
    
    public override string ToString() => $"Play {{{Sound.Name}}} sound until done;";
}