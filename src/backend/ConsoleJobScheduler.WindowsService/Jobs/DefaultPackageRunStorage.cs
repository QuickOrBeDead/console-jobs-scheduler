namespace ConsoleJobScheduler.WindowsService.Jobs;

using System.Collections.ObjectModel;

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

    public void AppendToLog(string packageName, string jobRunId, string content)
    {
        var packageLogFolder = GetPackageLogFolder(packageName);
        if (!Directory.Exists(packageLogFolder))
        {
            Directory.CreateDirectory(packageLogFolder);
        }

        var jobLogFile = GetJobLogFilePath(jobRunId, packageLogFolder);
        File.AppendAllText(jobLogFile, content + "\n");
    }

    public IList<string> GetLogLines(string packageName, string jobRunId)
    {
        if (string.IsNullOrWhiteSpace(packageName))
        {
            return new ReadOnlyCollection<string>(new List<string>(0));
        }

        var logFile = GetJobLogFilePathByPackageName(packageName, jobRunId);
        if (!File.Exists(logFile))
        {
            return new ReadOnlyCollection<string>(new List<string>(0));
        }

        return File.ReadAllLines(logFile);
    }

    public string GetAttachmentsPath(string packageName, string jobRunId)
    {
        var attachmentsPath = GetAttachmentsFolder(packageName, jobRunId);
        if (!Directory.Exists(attachmentsPath))
        {
            Directory.CreateDirectory(attachmentsPath);
        }

        return attachmentsPath;
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