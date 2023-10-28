namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using Models;

public interface IPackageRunStorage
{
    IList<LogLine> GetLogLines(string packageName, string jobRunId);

    IList<string> GetAttachmentNames(string packageName, string jobRunId);

    byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName);
}