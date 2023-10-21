namespace ConsoleJobScheduler.Messaging.Models;

public sealed class ConsoleLogMessage
{
    public ConsoleMessageLogType LogType { get; set; }

    public string? Message { get; set; }
}