using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace ConsoleJobScheduler.Core.Jobs;

[PersistJobDataAfterExecution]
public sealed class ConsoleAppPackageJob(IServiceProvider serviceProvider, ILogger<ConsoleAppPackageJob> logger)
    : IJob
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<ConsoleAppPackageJob> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task Execute(IJobExecutionContext context)
    {
        var package = context.JobDetail.GetPackageName();
        if (package == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"package\") is null");
        }

        var parameters = context.JobDetail.GetParameters();
        if (parameters == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"parameters\") is null");
        }

        var cancellationTokenSource = new CancellationTokenSource();
        try
        {
            async Task UpdateJobLastSignalTime(string jobRunId, IJobHistoryApplicationService jobHistoryApplicationService, CancellationToken c)
            {
                while (!c.IsCancellationRequested)
                {
                    try
                    {
                        await jobHistoryApplicationService.UpdateJobHistoryEntryLastSignalTime(jobRunId, DateTime.UtcNow, c);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating job history last signal time. Job run id: {jobRunId}", jobRunId);
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
            var signalTask = UpdateJobLastSignalTime(context.FireInstanceId, serviceScope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>(), cancellationTokenSource.Token);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job history last signal time.");
            }
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }
    }
}