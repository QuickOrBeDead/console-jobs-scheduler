namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public class JobRunAttachment
{
    public long Id { get; set; }

    public Guid? EmailId { get; set; }

    public JobRunEmail? JobRunEmail { get; set; }

    public byte[] Content { get; set; }

    public string JobRunId { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public DateTime CreateDate { get; set; }

    protected JobRunAttachment()
    {
    }

    public JobRunAttachment(string jobRunId, byte[] content, string fileName, string contentType)
    {
        JobRunId = jobRunId ?? throw new ArgumentNullException(nameof(jobRunId)); 
        Content = content ?? throw new ArgumentNullException(nameof(content));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        CreateDate = DateTime.UtcNow;
    }
}