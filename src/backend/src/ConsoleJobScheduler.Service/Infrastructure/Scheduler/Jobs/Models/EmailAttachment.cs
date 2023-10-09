namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;

public sealed class EmailAttachment
{
    public string FileName { get; set; }

    public string ContentType { get; set; }
}