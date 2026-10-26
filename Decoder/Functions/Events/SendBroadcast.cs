namespace ScratchCompiler.Decoder.Functions.Events;

public class SendBroadcast(Broadcast broadcast) : Function
{
    public const string ScratchCode = "event_broadcast";

    public override string ToString() => $"Broadcast {broadcast.Name}";
}