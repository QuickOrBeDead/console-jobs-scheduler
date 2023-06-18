namespace ConsoleJobScheduler.WindowsService.Jobs;

public sealed class DefaultPackageStorage : IPackageStorage
{
    private readonly string _rootPath;

    public DefaultPackageStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootPath));
        }

        _rootPath = rootPath;
    }

    public Stream GetPackageStream(string name)
    {
        return File.OpenRead(Path.Combine(GetPackagesPath(), $"{name}.zip"));
    }

    public IList<string> GetPackages()
    {
        return Directory.EnumerateFiles(GetPackagesPath(), "*.zip", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(x => x)
            .ToList()!;
    }

    private string GetPackagesPath()
    {
        return Path.Combine(_rootPath, "packages");
    }
}