namespace ConsoleJobScheduler.Core.Infra.EMail.Model;

public sealed class AttachmentModel
{
    private AttachmentModel()
    {
    }

    public string? JobRunId { get; private set; }

    public byte[]? FileContent { get; private set; }

    public string? FileName { get; private set; }

    public string? ContentType { get; private set; }

    public Guid? EmailId { get; private set; }

    public void SetEmailId(Guid id)
    {
        EmailId = id;
    }

    public static AttachmentModel Create(string jobRunId, string fileName, byte[] fileContent, string contentType)
    {
        // TODO: validation
        return new AttachmentModel
        {
            JobRunId = jobRunId,
            FileName = fileName,
            FileContent = fileContent,
            ContentType = contentType
        };
    }
}