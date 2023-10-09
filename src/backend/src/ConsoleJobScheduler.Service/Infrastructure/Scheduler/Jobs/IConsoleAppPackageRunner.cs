namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

public interface IConsoleAppPackageRunner
{
    Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}