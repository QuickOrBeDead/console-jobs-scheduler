namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using Models;

public interface IPackageStorage
{
    Stream GetPackageStream(string name);

    IList<string> GetPackages();

    PackageDetailsModel? GetPackageDetails(string packageName);

    Task SavePackage(string packageName, byte[] content);
}