namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

public sealed class EmailModel
{
    private IList<AttachmentModel>? _attachments;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string? JobRunId { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public IList<AttachmentModel> Attachments
    {
        get => _attachments ??= new List<AttachmentModel>();
        set => _attachments = value;
    }

    public string? To { get; set; }

    public string? CC { get; set; }

    public string? Bcc { get; set; }
}