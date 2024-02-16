using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Runner;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ConsoleJobScheduler.Core.Jobs;

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

        var cancellationTokenSource = new CancellationTokenSource();
        try
        {
            async Task UpdateJobLastSignalTime(string jobRunId, IJobApplicationService jobApplicationService, CancellationToken c)
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        await jobApplicationService.UpdateJobHistoryEntryLastSignalTime(jobRunId, DateTime.UtcNow, c);
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

            using var serviceScope = _serviceProvider.CreateScope();
            var signalTask = UpdateJobLastSignalTime(context.FireInstanceId, serviceScope.ServiceProvider.GetRequiredService<IJobApplicationService>(), cancellationTokenSource.Token);

            await serviceScope.ServiceProvider.GetRequiredService<IConsoleAppPackageRunner>().Run(context.FireInstanceId, package, parameters, context.CancellationToken).ConfigureAwait(false);

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