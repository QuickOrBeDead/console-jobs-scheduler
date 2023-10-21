namespace ConsoleJobScheduler.Messaging;

using ConsoleJobScheduler.Messaging.Models;
using System.Text.Json;

public sealed class ConsoleMessageWriter
{
    public void WriteEmail(EmailMessage emailMessage)
    {
        Console.WriteLine(JsonSerializer.Serialize(new ConsoleMessage
                                                       {
                                                           MessageType = ConsoleMessageType.Email,
                                                           Message = emailMessage
                                                       }));
    }

    public void WriteLog(ConsoleMessageLogType logType, string message)
    {

        Console.WriteLine(JsonSerializer.Serialize(new ConsoleMessage
                                                       {
                                                           MessageType = ConsoleMessageType.Log,
                                                           Message = new ConsoleLogMessage
                                                                         {
                                                                             LogType = logType,
                                                                             Message = message
                                                                         }
                                                       }));
    }
}