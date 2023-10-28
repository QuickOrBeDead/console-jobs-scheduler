namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using System.Linq;

public sealed class DefaultPackageRunStorage : IPackageRunStorage
{
    private readonly string _rootPath;

    public DefaultPackageRunStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootPath));
        }

        _rootPath = rootPath;
    }

    public IList<string> GetAttachmentNames(string packageName, string jobRunId)
    {
        var attachmentsPath = GetAttachmentsFolder(packageName, jobRunId);
        if (!Directory.Exists(attachmentsPath))
        {
            return new List<string>(0);
        }

        return Directory.EnumerateFiles(attachmentsPath, "*.*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList()!;
    }

    public byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName)
    {
        var attachmentsPath = Path.Combine(GetAttachmentsFolder(packageName, jobRunId), attachmentName);
        if (!File.Exists(attachmentsPath))
        {
            return null;
        }

        return File.ReadAllBytes(attachmentsPath);
    }

    private string GetAttachmentsFolder(string packageName, string jobRunId)
    {
        return Path.Combine(_rootPath, "Attachments", packageName, jobRunId);
    }
}