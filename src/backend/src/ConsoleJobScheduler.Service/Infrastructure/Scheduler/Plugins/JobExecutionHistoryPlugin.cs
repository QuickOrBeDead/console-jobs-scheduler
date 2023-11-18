namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins;

using ConsoleJobScheduler.Service.Infrastructure.Extensions;
using ConsoleJobScheduler.Service.Infrastructure.Logging;

using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

public sealed class JobExecutionHistoryPlugin : ISchedulerPlugin, IJobListener
{
    public const string PluginConfigurationProperty = "quartz.plugin.jobExecutionHistory.type";

    private IScheduler? _scheduler;

    private JobStoreDelegate? _jobStoreDelegate;

    private ILogger<JobExecutionHistoryPlugin>? _logger;

    public string Name { get; private set; } = default!;

    public Task Initialize(string pluginName, IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        Name = pluginName;
        _scheduler = scheduler;
        _scheduler.ListenerManager.AddJobListener(this, EverythingMatcher<JobKey>.AllJobs());

        return Task.CompletedTask;
    }

    public Task Start(CancellationToken cancellationToken = default)
    {
        _logger = LoggerFactory.CreateLogger<JobExecutionHistoryPlugin>();

        var scheduler = GetScheduler();
        var jobStore = scheduler.GetJobStore();
        _jobStoreDelegate = new JobStoreDelegate(scheduler, jobStore.GetDbAccessor(), jobStore.DataSource, jobStore.TablePrefix);

        scheduler.AddJobStoreDelegate(_jobStoreDelegate);

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
            await GetJobStoreDelegate().InsertJobHistoryEntry(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger?.LogError(e, "error on JobToBeExecuted"));
        }
    }

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await GetJobStoreDelegate().UpdateJobHistoryEntryVetoed(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger?.LogError(e, "error on JobExecutionVetoed"));
        }
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await GetJobStoreDelegate().UpdateJobHistoryEntryCompleted(context, jobException, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            DoNotThrow(() => _logger?.LogError(e, "error on JobWasExecuted"));
        }
    }

    private JobStoreDelegate GetJobStoreDelegate()
    {
        if (_jobStoreDelegate == null)
        {
            throw new InvalidOperationException("JobStoreDelegate object is null");
        }

        return _jobStoreDelegate;
    }

    private IScheduler GetScheduler()
    {
        if (_scheduler == null)
        {
            throw new InvalidOperationException("Scheduler object is null");
        }

        return _scheduler;
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