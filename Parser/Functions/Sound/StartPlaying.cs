using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Sound;

public class StartPlaying : Function
{
    public const string ScratchCode = "sound_play";
    
    public Parser.Sound Sound { get; }
    
    private StartPlaying(Parser.Sound sound) => Sound = sound;

    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static StartPlaying Parse(JsonObject json, JsonObject list, Target target) {
        var @ref = json.GetInputReference("SOUND_MENU", list);
        var name = @ref.GetField("SOUND_MENU");
        var sound = target.Sounds.FirstOrDefault(c => c.Name == name) ?? throw new KeyNotFoundException(name);
        return new StartPlaying(sound);
    }
    
    public override string ToString() => $"Start playing {{{Sound.Name}}} sound;";
}