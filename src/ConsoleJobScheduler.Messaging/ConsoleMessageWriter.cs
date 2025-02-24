namespace ConsoleJobScheduler.Messaging;

using Models;
using System.Text.Json;

public static class ConsoleMessageWriter
{
    public const string JsonPrefix = "##[json]";

    public static void WriteEmail(EmailMessage emailMessage)
    {
        Console.WriteLine(GetEmailMessage(emailMessage));
    }
    
    public static void WriteLog(ConsoleMessageLogType logType, string message)
    {
        Console.WriteLine(GetLogMessage(logType, message));
    }

    internal static string GetEmailMessage(EmailMessage emailMessage)
    {
        var json = JsonSerializer.Serialize(new ConsoleMessage
        {
            MessageType = ConsoleMessageType.Email,
            Message = emailMessage
        });
        return $"{JsonPrefix}{json}";
    }
    
    internal static string GetLogMessage(ConsoleMessageLogType logType, string message)
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
       return $"{JsonPrefix}{json}";
    }
}