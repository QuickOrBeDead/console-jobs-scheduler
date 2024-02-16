namespace ConsoleJobScheduler.Core.Infra.EMail.Model;

public sealed class EmailModel
{
    private IList<AttachmentModel>? _attachments;

    private EmailModel()
    {
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string JobRunId { get; private set; }

    public string? Subject { get; private set; }

    public string? Body { get; private set; }

    public IList<AttachmentModel> Attachments
    {
        get
        {
            _attachments ??= new List<AttachmentModel>();

            return _attachments.AsReadOnly();
        }
        private set => _attachments = value;
    }

    public string? To { get; private set; }

    public string? CC { get; private set; }

    public string? Bcc { get; private set; }

    public void AddAttachment(string fileName, string fileContent, string contentType)
    {
        var attachmentModel = AttachmentModel.Create(JobRunId, fileName, fileContent, contentType);
        attachmentModel.SetEmailId(Id);

        Attachments.Add(attachmentModel);
    }

    public static EmailModel Create(string jobRunId, string subject, string body, string to, string? cc = null, string? bcc = null)
    {
        // TODO: validation
        return new EmailModel
        {
            JobRunId = jobRunId,
            Subject = subject,
            Body = body,
            To = to,
            CC = cc,
            Bcc = bcc
        };
    }
}