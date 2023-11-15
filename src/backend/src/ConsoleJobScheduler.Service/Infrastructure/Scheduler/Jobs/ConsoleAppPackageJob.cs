namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using ConsoleJobScheduler.Service.Infrastructure.Extensions;

using Quartz;

[PersistJobDataAfterExecution]
public sealed class ConsoleAppPackageJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public ConsoleAppPackageJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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

        var jobStoreDelegate = context.Scheduler.GetJobStoreDelegate();
        var cancellationTokenSource = new CancellationTokenSource();
        try
        {
            async Task UpdateJobLastSignalTime(string jobRunId, CancellationToken c)
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        await jobStoreDelegate.UpdateJobHistoryEntryLastSignalTime(jobRunId, c);
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

            using var serviceScope = _serviceProvider.CreateScope();
            await serviceScope.ServiceProvider.GetRequiredService<IConsoleAppPackageRunner>().Run(jobStoreDelegate, context.FireInstanceId, package, parameters, context.CancellationToken).ConfigureAwait(false);

            await cancellationTokenSource.CancelAsync();

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
            await cancellationTokenSource.CancelAsync();
        }
    }
}