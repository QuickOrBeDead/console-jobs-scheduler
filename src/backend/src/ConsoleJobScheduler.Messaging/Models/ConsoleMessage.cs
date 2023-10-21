namespace ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleMessage
{
    public ConsoleMessageType MessageType { get; set; }

    public object? Message { get; set; }
}