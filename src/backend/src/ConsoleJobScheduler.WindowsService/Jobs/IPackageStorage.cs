namespace ConsoleJobScheduler.WindowsService.Jobs;

using ConsoleJobScheduler.WindowsService.Jobs.Models;

public interface IPackageStorage
{
    Stream GetPackageStream(string name);

    IList<string> GetPackages();

    PackageDetailsModel? GetPackageDetails(string packageName);

    Task SavePackage(string packageName, byte[] content);
}