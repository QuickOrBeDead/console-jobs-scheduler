namespace ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleLogMessage
{
    public ConsoleMessageLogType LogType { get; set; }

    public string? Message { get; set; }

    public static ConsoleMessage CreateConsoleMessage(ConsoleMessageLogType logType, string messageText)
    {
        return new ConsoleMessage
                   {
                       MessageType = ConsoleMessageType.Log,
                       Message = new ConsoleLogMessage {LogType = logType, Message = messageText}
                   };
    }
}