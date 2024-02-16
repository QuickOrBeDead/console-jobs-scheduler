namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IConsoleAppPackageRunner
{
    Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}