namespace ScratchCompiler.Parser.Functions.Events;

public class SendBroadcastAndWait(Broadcast broadcast) : Function
{
    public const string ScratchCode = "event_broadcastandwait";

    public override string ToString() => $"Broadcast {{{broadcast.Name}}} and wait;";
}