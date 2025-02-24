namespace ConsoleJobScheduler.Messaging.Models;

public sealed class EmailAttachment
{
    public string? FileContent { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public byte[] GetContentBytes()
    {
        return string.IsNullOrEmpty(FileContent) ? Array.Empty<byte>() : Convert.FromBase64String(FileContent);
    }
}