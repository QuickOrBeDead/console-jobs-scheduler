namespace ConsoleJobScheduler.WindowsService.Jobs.Models;

public sealed class EmailAttachment
{
    public string FileName { get; set; }

    public string ContentType { get; set; }
}