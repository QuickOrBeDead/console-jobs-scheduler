namespace ConsoleJobScheduler.Messaging;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleMessageReader
{
    public ConsoleMessage? ReadMessage(string jsonText)
    {
        var messageNode = JsonNode.Parse(jsonText);
        if (messageNode == null)
        {
            return null;
        }

        var messageType = (ConsoleMessageType?)messageNode[nameof(ConsoleMessage.MessageType)]?.GetValue<int>();
        if (messageType == null)
        {
            return null;
        }

        if (messageType == ConsoleMessageType.Log)
        {
            return new ConsoleMessage
                       {
                           MessageType = messageType.Value,
                           Message = messageNode[nameof(ConsoleMessage.Message)].Deserialize<ConsoleLogMessage>()
                       };
        }

        if (messageType == ConsoleMessageType.Email)
        {
            return new ConsoleMessage
                       {
                           MessageType = messageType.Value,
                           Message = messageNode[nameof(ConsoleMessage.Message)].Deserialize<ConsoleLogMessage>()
                       };
        }

        return null;
    }
}