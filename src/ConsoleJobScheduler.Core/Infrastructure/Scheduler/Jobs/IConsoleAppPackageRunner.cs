using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs;

public interface IConsoleAppPackageRunner
{
    Task Run(IJobStoreDelegate jobStoreDelegate, string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}