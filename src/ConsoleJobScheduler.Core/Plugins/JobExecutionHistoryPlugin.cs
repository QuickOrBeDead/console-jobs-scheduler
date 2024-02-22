using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Runner.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace ConsoleJobScheduler.Core.Plugins;

public sealed class JobExecutionHistoryPlugin : ISchedulerPlugin, IJobListener
{
    public const string PluginConfigurationProperty = "quartz.plugin.jobExecutionHistory.type";

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobExecutionHistoryPlugin> _logger;

    public string Name { get; private set; } = null!;

    public JobExecutionHistoryPlugin(IServiceProvider serviceProvider, ILogger<JobExecutionHistoryPlugin> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        Name = pluginName;
        scheduler.ListenerManager.AddJobListener(this, EverythingMatcher<JobKey>.AllJobs());

        return Task.CompletedTask;
    }

    public Task Start(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Shutdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var jobExecutionHistory = new JobExecutionHistory(
                context.FireInstanceId,
                context.Scheduler.SchedulerName,
                context.Scheduler.SchedulerInstanceId,
                context.JobDetail.GetPackageName(),
                context.JobDetail.Key.Name,
                context.JobDetail.Key.Group,
                context.Trigger.Key.Name,
                context.Trigger.Key.Group,
                context.ScheduledFireTimeUtc?.UtcDateTime,
                context.FireTimeUtc.UtcDateTime,
                context.ScheduledFireTimeUtc?.UtcDateTime ?? context.FireTimeUtc.UtcDateTime,
                context.Trigger.GetFireTimeAfter(context.ScheduledFireTimeUtc)?.UtcDateTime,
                (context.Trigger as ICronTrigger)?.CronExpressionString);

            using var scope = _serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>().InsertJobHistoryEntry(jobExecutionHistory, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger.LogError(e, "error on JobToBeExecuted"));
        }
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>().UpdateJobHistoryEntryVetoed(context.FireInstanceId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger.LogError(e, "error on JobExecutionVetoed"));
        }
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>().UpdateJobHistoryEntryCompleted(context.FireInstanceId, context.JobRunTime, jobException, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger.LogError(e, "error on JobWasExecuted"));
        }
    }

    private static void DoNotThrow(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
        }
        catch
        {
            // empty
        }
    }
}