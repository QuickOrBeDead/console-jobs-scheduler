namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobRunEmail
{
    private IList<JobRunEmailAttachment>? _attachments;

    public Guid Id { get; private set; }

    public string JobRunId { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public IList<JobRunEmailAttachment> Attachments
    {
        get
        {
            _attachments ??= new List<JobRunEmailAttachment>();

            return _attachments.AsReadOnly();
        }
        private set => _attachments = value;
    }

    public string To { get; private set; }

    public string? CC { get; private set; }

    public string? Bcc { get; private set; }

    public JobRunEmail(string jobRunId, string subject, string body, string to, string? cc = null, string? bcc = null)
    {
        Id = Guid.NewGuid();
        JobRunId = jobRunId;
        Subject = subject;
        Body = body;
        To = to;
        CC = cc;
        Bcc = bcc;
    }

    public void AddAttachment(string fileName, string fileContent, string contentType)
    {
        var attachment = new JobRunEmailAttachment(Id, JobRunId, fileContent, fileName, contentType);

        Attachments.Add(attachment);
    }
}