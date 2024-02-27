namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobRunEmailAttachment : JobRunAttachment
{
    private JobRunEmailAttachment()
        : base()
    {
    }

    public JobRunEmailAttachment(Guid emailId, string jobRunId, byte[] content, string fileName, string contentType) : 
        base(jobRunId, content, fileName, contentType)
    {
        EmailId = emailId;
    }
}