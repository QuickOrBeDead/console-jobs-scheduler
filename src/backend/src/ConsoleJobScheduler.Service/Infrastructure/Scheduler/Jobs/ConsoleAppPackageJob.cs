namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using ConsoleJobScheduler.Service.Infrastructure.Extensions;

using Quartz;

[PersistJobDataAfterExecution]
public sealed class ConsoleAppPackageJob : IJob
{
    private readonly IConsoleAppPackageRunner _consoleAppPackageRunner;

    public ConsoleAppPackageJob(IConsoleAppPackageRunner consoleAppPackageRunner)
    {
        _consoleAppPackageRunner = consoleAppPackageRunner ?? throw new ArgumentNullException(nameof(consoleAppPackageRunner));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobData = context.JobDetail.JobDataMap;
        var package = jobData.GetString("package");
        if (package == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"package\") is null");
        }

        var parameters = jobData.GetString("parameters");
        if (parameters == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"parameters\") is null");
        }

        var jobHistoryDelegate = context.Scheduler.GetJobHistoryDelegate();
        var cancellationTokenSource = new CancellationTokenSource();
        try
        {
            async Task UpdateJobLastSignalTime(string jobRunId, CancellationToken c)
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        await jobHistoryDelegate.UpdateJobHistoryEntryLastSignalTime(jobRunId, c);
                    }
                    catch
                    {
                       // Empty
                       // TODO: log
                    }

                    try
                    {
                        // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
                        await Task.Delay(10_000, c).ContinueWith(_ => Task.CompletedTask);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods
                    }
                    catch (OperationCanceledException)
                    {
                        // Empty
                    }
                }
            }

            var signalTask = UpdateJobLastSignalTime(context.FireInstanceId, cancellationTokenSource.Token);
            await _consoleAppPackageRunner.Run(jobHistoryDelegate, context.FireInstanceId, package, parameters, context.CancellationToken).ConfigureAwait(false);

            cancellationTokenSource.Cancel();

            try
            {
                await signalTask;
            }
            catch (OperationCanceledException)
            {
                // Empty
            }
            catch (Exception)
            {
                // Empty
                // TODO: log
            }
        }
        finally
        {
            cancellationTokenSource.Cancel();
        }
    }
}