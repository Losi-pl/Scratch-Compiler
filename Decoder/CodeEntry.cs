using System.Collections.Frozen;
using ScratchCompiler.Decoder.Entries;

namespace ScratchCompiler.Decoder;

public abstract class CodeEntry
{
    public static readonly FrozenSet<string> EntryCodes = [OnStart.ScratchCode, OnStageClick.ScratchCode, OnActorClick.ScratchCode, OnKeyPress.ScratchCode, OnSceneChangedTo.ScratchCode, "event_whengreaterthan", "event_whenbroadcastreceived", ];
}