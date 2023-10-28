namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

public sealed class AttachmentModel
{
    public string? JobRunId { get; set; }

    public string? FileContent { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public Guid? EmailId { get; set; }
}