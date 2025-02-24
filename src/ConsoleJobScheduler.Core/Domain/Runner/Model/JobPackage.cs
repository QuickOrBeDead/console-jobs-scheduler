namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobPackage
{
    public string Name { get; internal set; }

    public string Author { get; internal set; }

    public string Version { get; internal set; }

    public string Description { get; internal set; }

    public string FileName { get; internal set; }

    public string Arguments { get; internal set; }

    public byte[] Content { get; internal set; }

    public DateTime CreateDate { get; internal set; }

    public JobPackageRun CreateJobPackageRun(string tempRootPath)
    {
        return new JobPackageRun(Name, Content, FileName, Arguments, tempRootPath);
    }
}