namespace ConsoleJobScheduler.Messaging;

using ConsoleJobScheduler.Messaging.Models;
using System.Text.Json;

public sealed class ConsoleMessageWriter
{
    public const string JsonPrefix = "##[json]";
    
    public void WriteEmail(EmailMessage emailMessage)
    {
        var json = JsonSerializer.Serialize(new ConsoleMessage
                                                     {
                                                         MessageType = ConsoleMessageType.Email,
                                                         Message = emailMessage
                                                     });
        Console.WriteLine($"{JsonPrefix}{json}");
    }

    public void WriteLog(ConsoleMessageLogType logType, string message)
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