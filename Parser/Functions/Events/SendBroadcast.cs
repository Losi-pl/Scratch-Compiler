using System.Text.Json.Nodes;

namespace ScratchCompiler.Parser.Functions.Events;

public class SendBroadcast : Function
{
    public const string ScratchCode = "event_broadcast";

    public Broadcast Broadcast { get; }
    
    private SendBroadcast(Broadcast broadcast) => Broadcast = broadcast;
    
    [FunctionParser(ScratchCode)] // ReSharper disable once UnusedMember.Local
    private static SendBroadcast Parse(JsonObject json, JsonObject list, Target _) {
        var code = json["inputs"]?["BROADCAST_INPUT"]?[1]?[2]?.GetValue<string>() ?? string.Empty;
        var br = SyntaxTree.Current.project.Broadcasts.FindByScratchCode(code);
        if(br is null)
            throw new NullReferenceException($"Broadcast \"{code}\" not found");
        return new SendBroadcast(br);
    }
    
    public override string ToString() => $"Broadcast {{{Broadcast.Name}}};";
}