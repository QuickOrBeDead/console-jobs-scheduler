namespace ConsoleJobScheduler.Core.Infra.EMail.Model;

public sealed class EmailModel
{
    private IList<AttachmentModel>? _attachments;

    private EmailModel(string jobRunId, string subject, string body, string to)
    {
        Id = Guid.NewGuid();
        JobRunId = jobRunId;
        To = to;
        Body = body;
        Subject = subject;
    }

    public Guid Id { get; private init; }

    public string JobRunId { get; private init; }

    public string Subject { get; private init; }

    public string Body { get; private init; }

    private IList<AttachmentModel> Attachments
    {
        get { return _attachments ??= new List<AttachmentModel>(); }
    }

    public string To { get; private set; }

    public string? CC { get; private set; }

    public string? Bcc { get; private set; }

    public IList<AttachmentModel> GetAttachments()
    {
        return Attachments.AsReadOnly();
    }
    
    public void AddAttachment(string fileName, string fileContent, string contentType)
    {
        var attachmentModel = AttachmentModel.Create(JobRunId, fileName, fileContent, contentType);
        attachmentModel.SetEmailId(Id);

        Attachments.Add(attachmentModel);
    }

    public static EmailModel Create(string jobRunId, string subject, string body, string to, string? cc = null, string? bcc = null)
    {
        // TODO: validation
        return new EmailModel(jobRunId, subject, body, to)
        {
            CC = cc,
            Bcc = bcc
        };
    }
}