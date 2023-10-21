namespace ConsoleJobScheduler.Messaging.Models;

public sealed class EmailAttachment
{
    public string? FileContent { get; set; }

    public string? ContentType { get; set; }
}