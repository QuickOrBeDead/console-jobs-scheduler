namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using System.Collections.ObjectModel;
using System.Linq;

using Models;

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

    public IList<LogLine> GetLogLines(string packageName, string jobRunId)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return new ReadOnlyCollection<LogLine>(new List<LogLine>(0));
        }

        var logFile = GetJobLogFilePathByPackageName(packageName, jobRunId);
        if (!File.Exists(logFile))
        {
            return new ReadOnlyCollection<LogLine>(new List<LogLine>(0));
        }

        return File.ReadAllLines(logFile)
            .Select(x => new LogLine
                               {
                                   Message = x,
                                   IsError = !string.IsNullOrWhiteSpace(x) && x.StartsWith("##[error] ", StringComparison.InvariantCultureIgnoreCase)
                               })
            .ToList();
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

    private string GetJobLogFilePathByPackageName(string packageName, string jobRunId)
    {
        return GetJobLogFilePath(jobRunId, GetPackageLogFolder(packageName));
    }

    private static string GetJobLogFilePath(string jobRunId, string packageLogFolder)
    {
        return Path.Combine(packageLogFolder, $"{jobRunId}.log");
    }

    private string GetPackageLogFolder(string packageName)
    {
        return Path.Combine(_rootPath, "Logs", packageName);
    }

    private string GetAttachmentsFolder(string packageName, string jobRunId)
    {
        return Path.Combine(_rootPath, "Attachments", packageName, jobRunId);
    }
}