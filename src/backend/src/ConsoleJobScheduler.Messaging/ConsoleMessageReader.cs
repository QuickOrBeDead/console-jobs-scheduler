namespace ConsoleJobScheduler.Messaging;

using System.Text.Json;
using System.Text.Json.Nodes;

using ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleMessageReader
{
    public ConsoleMessage? ReadMessage(string messageText)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            return ConsoleLogMessage.CreateConsoleMessage(ConsoleMessageLogType.Info, "[empty]");
        }

        var isJson = messageText.StartsWith(ConsoleMessageWriter.JsonPrefix, StringComparison.InvariantCultureIgnoreCase);
        var message = isJson ? messageText[ConsoleMessageWriter.JsonPrefix.Length..] : messageText;
        if (!isJson)
        {
            return ConsoleLogMessage.CreateConsoleMessage(ConsoleMessageLogType.Info, message);
        }
        
        JsonNode? messageNode;
        try
        {
            messageNode = JsonNode.Parse(message);
            if (messageNode == null)
            {
                return null;
            }
        }
        catch
        {
            return ConsoleLogMessage.CreateConsoleMessage(ConsoleMessageLogType.Error, message);
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
                           Message = messageNode[nameof(ConsoleMessage.Message)].Deserialize<EmailMessage>()
                       };
        }

        return null;
    }
}