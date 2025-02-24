namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobRunEmail
{
    private IList<JobRunEmailAttachment> _attachments = new List<JobRunEmailAttachment>();

    public Guid Id { get; private set; }

    public string JobRunId { get; private set; }

    public string Subject { get; private set; }

    public string Body { get; private set; }

    public IList<JobRunEmailAttachment> Attachments
    {
        get
        {
            return _attachments.AsReadOnly();
        }
        private set => _attachments = value;
    }

    public string To { get; private set; }

    public string? CC { get; private set; }

    public string? Bcc { get; private set; }

    public bool IsSent { get; set; }

    public DateTime CreateDate { get; set; }

    internal JobRunEmail()
    {
    }

    public JobRunEmail(Guid id, string jobRunId, string subject, string body, string to, string? cc = null, string? bcc = null)
    {
        Id = id;
        JobRunId = jobRunId;
        Subject = subject;
        Body = body;
        To = to;
        CC = cc;
        Bcc = bcc;
        CreateDate = DateTime.UtcNow;
    }

    public void AddAttachment(string fileName, byte[] fileContent, string contentType)
    {
        _attachments.Add(new JobRunEmailAttachment(Id, JobRunId, fileContent, fileName, contentType));
    }
}