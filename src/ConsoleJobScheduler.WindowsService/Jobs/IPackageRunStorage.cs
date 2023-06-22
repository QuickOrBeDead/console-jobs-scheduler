namespace ConsoleJobScheduler.WindowsService.Jobs;

public interface IPackageRunStorage
{
    void AppendToLog(string packageName, string jobRunId, string content);

    IList<string> GetLogLines(string packageName, string jobRunId);

    string GetAttachmentsPath(string packageName, string jobRunId);

    IList<string> GetAttachmentNames(string packageName, string jobRunId);

    byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName);
}