namespace ConsoleJobScheduler.WindowsService.Jobs;

using ConsoleJobScheduler.WindowsService.Jobs.Models;

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

    public PackageDetailsModel? GetPackageDetails(string packageName)
    {
        // TODO: validate package name

        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
        }

        return Directory.EnumerateFiles(GetPackagesPath(), $"{packageName}.zip", SearchOption.TopDirectoryOnly)
            .Select(x => new PackageDetailsModel
                             {
                                 Name = Path.GetFileNameWithoutExtension(x),
                                 Path = x,
                                 ModifyDate = File.GetLastWriteTime(x)
                             })
            .FirstOrDefault();
    }

    public Task SavePackage(string packageName, byte[] content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
        }

        return File.WriteAllBytesAsync(Path.Combine(GetPackagesPath(), $"{packageName}.zip"), content);
    }

    private string GetPackagesPath()
    {
        var path = Path.Combine(_rootPath, "packages");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}