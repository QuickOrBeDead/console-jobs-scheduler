namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobRunEmailAttachment : JobRunAttachment
{
    public Guid EmailId { get; private set; }

    public JobRunEmailAttachment(Guid emailId, string jobRunId, string fileContent, string fileName, string contentType) : 
        base(jobRunId, fileContent, fileName, contentType)
    {
        EmailId = emailId;
    }
}