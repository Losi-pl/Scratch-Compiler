using System.Collections.Frozen;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public abstract class CodeEntry
{
    public static readonly FrozenSet<string> EntryCodes = [OnStart.ScratchCode, OnStageClick.ScratchCode, OnActorClick.ScratchCode, OnKeyPress.ScratchCode, OnBackgroundChangedTo.ScratchCode, OnGraterThan.ScratchCode, "event_whenbroadcastreceived", ];

    internal static CodeEntry LoadEntry(JsonObject json)
    {
        var opcode = json["opcode"]?.GetValue<string>() ?? string.Empty;
        if (CodeEntry.EntryCodes.Contains(opcode))
            switch (opcode)
            {
                case OnStart.ScratchCode:
                    return new OnStart();
                
                case OnActorClick.ScratchCode:
                    return new OnActorClick();
                
                case OnStageClick.ScratchCode:
                    return new OnStageClick();
                
                case OnBackgroundChangedTo.ScratchCode:
                {
                    var backdropName = json["fields"]?["BACKDROP"]?[0]?.GetValue<string>() ?? string.Empty;
                    var scene = Project._currentProject.project.Scene;
                    return new OnBackgroundChangedTo(scene.Costumes.FirstOrDefault(b => b.Name == backdropName)!);
                }
                
                case OnKeyPress.ScratchCode:
                {
                    var keyCode = json["fields"]?["KEY_OPTION"]?[0]?.GetValue<string>();
                    var key = keyCode switch
                    {
                        "space" => KeyOption.Space, "any" => KeyOption.Any,
                        "up arrow" => KeyOption.UpArrow, "down arrow" => KeyOption.DownArrow,
                        "left arrow" => KeyOption.LeftArrow, "right arrow" => KeyOption.RightArrow,
                        { Length: 1 } letter => letter[0] switch
                        {
                            >= 'a' and <= 'z' => (KeyOption)((int)KeyOption.A + (letter[0] - 'a')),
                            >= '0' and <= '9' => (KeyOption)((int)KeyOption.Num0 + (letter[0] - '0')),
                            _ => KeyOption.Any
                        },
                        _ => KeyOption.Any
                    };
                    return new OnKeyPress(key);
                }

                case OnGraterThan.ScratchCode:
                {
                    var source = json["fields"]?["WHENGREATERTHANMENU"]?[0]?.GetValue<string>() switch {
                        "LOUDNESS" => OnGraterThan.InputType.Loudness,
                        "TIMER" => OnGraterThan.InputType.Timer,
                        _ => throw new Exception()
                    };
                    var threshold = float.TryParse(json["inputs"]?["VALUE"]?[1]?[1]?.GetValue<string>(), out var f) ? f : float.NaN;
                    
                    return new OnGraterThan(source, threshold);
                }
                default:
                    return new UnknownEntry(json);
            }
        
        return new Dormant();
    }
}