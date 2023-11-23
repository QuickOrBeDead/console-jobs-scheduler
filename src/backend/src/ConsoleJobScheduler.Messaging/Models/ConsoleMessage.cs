namespace ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleMessage
{
    public ConsoleMessageType MessageType { get; init; }

    public object? Message { get; init; }
}