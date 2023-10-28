namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using Models;

public interface IPackageRunStorage
{
    void AddLog(string packageName, string jobRunId, string content, bool isError);

    IList<LogLine> GetLogLines(string packageName, string jobRunId);

    string GetAttachmentsPath(string packageName, string jobRunId);

    IList<string> GetAttachmentNames(string packageName, string jobRunId);

    byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName);
}