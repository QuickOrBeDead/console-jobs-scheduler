namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public class JobRunAttachment
{
    public string JobRunId { get; private set; }

    public string FileContent { get; private set; }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public JobRunAttachment(string jobRunId, string fileContent, string fileName, string contentType)
    {
        JobRunId = jobRunId ?? throw new ArgumentNullException(nameof(jobRunId));
        FileContent = fileContent ?? throw new ArgumentNullException(nameof(fileContent));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
    }
}