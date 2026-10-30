using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Events;

public class SendBroadcastAndWait : Function
{
    public const string ScratchCode = "event_broadcastandwait";

    public Broadcast Broadcast { get; }
    
    private SendBroadcastAndWait(Broadcast broadcast) => Broadcast = broadcast;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SendBroadcastAndWait Parse(JsonObject json, JsonObject list, Target _) {
        var code = json["inputs"]?["BROADCAST_INPUT"]?[1]?[2]?.GetValue<string>() ?? string.Empty;
        var br = SyntaxTree.Current.project.Broadcasts.FindByScratchCode(code);
        if(br is null)
            throw new NullReferenceException($"Broadcast \"{code}\" not found");
        return new SendBroadcastAndWait(br);
    }
    
    public override string ToString() => $"Broadcast {{{Broadcast.Name}}} and wait;";
}