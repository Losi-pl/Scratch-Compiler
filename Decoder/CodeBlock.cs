using System.Text.Json.Nodes;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public class CodeBlock
{
    private readonly CodeEntry _entry;
    private Function[] _functions;

    internal CodeBlock(JsonObject json, JsonObject entry)
    {
        var meCodeName = entry.GetPropertyName();
        var opcode = entry["opcode"]?.GetValue<string>() ?? string.Empty;
        if (CodeEntry.EntryCodes.Contains(opcode))
            switch (opcode)
            {
                case OnStart.ScratchCode:
                    _entry = new OnStart();
                    break;
                case OnSceneChangedTo.ScratchCode:
                {
                    var sceneName = entry["fields"]?["BACKDROP"]?[0]?.GetValue<string>() ?? string.Empty;
                    var scene = Project._currentProject.project.Scene;
                    _entry = new OnSceneChangedTo(scene);
                } break;
                default:
                    _entry = new UnknownEntry(entry);
                    break;
            }
        else
            _entry = new Dormant();
        
        var current = entry;
    }
}