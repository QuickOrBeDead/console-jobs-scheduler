namespace ConsoleJobScheduler.WindowsService.Jobs;

public interface IConsoleAppPackageRunner
{
    Task Run(string jobRunId, string packageName, string arguments);
}