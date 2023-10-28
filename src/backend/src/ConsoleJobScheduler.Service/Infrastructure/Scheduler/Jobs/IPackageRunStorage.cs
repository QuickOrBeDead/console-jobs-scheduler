namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

public interface IPackageRunStorage
{
    IList<string> GetAttachmentNames(string packageName, string jobRunId);

    byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName);
}