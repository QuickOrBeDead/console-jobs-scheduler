namespace ConsoleJobScheduler.Messaging;

using Models;
using System.Text.Json;

public static class ConsoleMessageWriter
{
    public const string JsonPrefix = "##[json]";

    public static void WriteEmail(EmailMessage emailMessage)
    {
        var json = JsonSerializer.Serialize(new ConsoleMessage
        {
            MessageType = ConsoleMessageType.Email,
            Message = emailMessage
        });
        Console.WriteLine($"{JsonPrefix}{json}");
    }

    public static void WriteLog(ConsoleMessageLogType logType, string message)
    {
        var json = JsonSerializer.Serialize(new ConsoleMessage
        {
            MessageType = ConsoleMessageType.Log,
            Message = new ConsoleLogMessage
            {
                LogType = logType,
                Message = message
            }
        });
        Console.WriteLine($"{JsonPrefix}{json}");
    }
}