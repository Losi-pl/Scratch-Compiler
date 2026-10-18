using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public abstract class CodeEntry
{
    public static readonly FrozenSet<string> EntryCodes = [
        OnStart.ScratchCode, 
        OnStageClick.ScratchCode, 
        OnActorClick.ScratchCode, 
        OnKeyPress.ScratchCode, 
        OnBackgroundChangedTo.ScratchCode, 
        OnGraterThan.ScratchCode, 
        OnBroadcast.ScratchCode
    ];

    internal static CodeEntry LoadEntry(JsonObject json, JsonObject list)
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
                    var backdropName = json.GetField("BACKDROP");
                    var scene = Project.Current.project.Scene;
                    return new OnBackgroundChangedTo(scene.Costumes.FirstOrDefault(b => b.Name == backdropName)!);
                }
                
                case OnKeyPress.ScratchCode:
                {
                    var keyCode = json.GetField("KEY_OPTION");
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
                    // ReSharper disable once StringLiteralTypo
                    var source = json.GetField("WHENGREATERTHANMENU") switch {
                        "LOUDNESS" => OnGraterThan.InputType.Loudness,
                        "TIMER" => OnGraterThan.InputType.Timer,
                        _ => throw new Exception()
                    };
                    var threshold = json.GetFloatInput(list, "VALUE");
                        
                    return new OnGraterThan(source, threshold);
                }
                case OnBroadcast.ScratchCode:
                {
                    var code = json["fields"]?["BROADCAST_OPTION"]?[1]?.GetValue<string>() ?? string.Empty;
                    var br = Project.Current.project.Broadcasts.FindByScratchCode(code);
                    if(br is null)
                        throw new NullReferenceException($"Broadcast \"{code}\" not found");
                    return new OnBroadcast(br);
                }
                default:
                    return new UnknownEntry(json);
            }
        
        return new Dormant();
    }
}