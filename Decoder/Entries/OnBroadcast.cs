namespace ScratchCompiler.Decoder.Entries;

public class OnBroadcast(Broadcast broadcast) : CodeEntry
{
    public const string ScratchCode = "event_whenbroadcastreceived";

    public Broadcast Broadcast => broadcast;
    

    public override string ToString() => $"When {{{Broadcast.Name}}} was broadcasted:";
}