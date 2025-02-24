using ConsoleJobScheduler.Core.Domain.Runner;

namespace ConsoleJobScheduler.Core.Tests.Application.Fakes;

public sealed class FakeConsoleAppPackageRunner : IConsoleAppPackageRunner
{
    public string? JobRunId { get; private set; }
    
    public string? PackageName { get; private set; }
    
    public string? Arguments { get; private set; }

    private readonly ManualResetEvent _manualResetEvent = new(false);
    
    public Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        JobRunId = jobRunId;
        PackageName = packageName;
        Arguments = arguments;

        _manualResetEvent.Set();
        return Task.CompletedTask;
    }

    public void WaitForCompletion(TimeSpan timeout)
    {
        _manualResetEvent.WaitOne(timeout);
    }
}