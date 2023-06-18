namespace ConsoleJobScheduler.WindowsService.Jobs.Models;

public sealed class EmailMessage
{
    private IList<EmailAttachment> _attachments;
    public string Subject { get; set; }

    public string Body { get; set; }

    public IList<EmailAttachment> Attachments
    {
        get => _attachments ??= new List<EmailAttachment>();
        set => _attachments = value;
    }

    public string To { get; set; }

    public string CC { get; set; }

    public string Bcc { get; set; }
}