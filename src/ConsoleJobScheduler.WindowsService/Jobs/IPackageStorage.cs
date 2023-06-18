namespace ConsoleJobScheduler.WindowsService.Jobs;

public interface IPackageStorage
{
    Stream GetPackageStream(string name);

    IList<string> GetPackages();
}