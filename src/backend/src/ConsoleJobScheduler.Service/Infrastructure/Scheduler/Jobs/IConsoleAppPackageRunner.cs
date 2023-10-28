using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins;

namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

public interface IConsoleAppPackageRunner
{
    Task Run(IJobHistoryDelegate jobHistoryDelegate, string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}