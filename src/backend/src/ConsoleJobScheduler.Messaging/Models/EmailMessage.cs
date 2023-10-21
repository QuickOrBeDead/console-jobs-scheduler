namespace ConsoleJobScheduler.Messaging.Models;


public sealed class EmailMessage
{
    private IList<EmailAttachment>? _attachments;

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public IList<EmailAttachment> Attachments
    {
        get => _attachments ??= new List<EmailAttachment>();
        set => _attachments = value;
    }

    public string? To { get; set; }

    public string? CC { get; set; }

    public string? Bcc { get; set; }

    public void AddAttachment(string contentType, byte[] fileBytes)
    {
        Attachments.Add(new EmailAttachment
                            {
                                ContentType = contentType,
                                FileContent = Convert.ToBase64String(fileBytes)
                            });
    }
}