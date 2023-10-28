namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using ConsoleJobScheduler.Service.Infrastructure.Data;
using ConsoleJobScheduler.Service.Infrastructure.Extensions;
using ConsoleJobScheduler.Service.Infrastructure.Logging;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

using MessagePipe;

using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;

public interface ISchedulerService
{
    Task Start(ILoggerFactory loggerFactory);

    Task Shutdown();

    TService GetService<TService>()
        where TService : notnull;

    void SubscribeToEvent<TEvent>(IAsyncMessageHandler<TEvent> handler);

    Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task<PagedResult<JobExecutionHistory>> GetJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

    Task AddOrUpdateJob(JobAddOrUpdateModel jobModel);

    Task<JobDetailModel?> GetJobDetail(JobKey jobKey);

    IList<string> GetPackages();

    PackageDetailsModel? GetPackageDetails(string packageName);

    Task<IList<JobListItemModel>> GetJobList();

    Task<(SchedulerMetaData, IReadOnlyCollection<SchedulerStateRecord>, JobExecutionStatistics)> GetStatistics();

    byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName);

    Task SavePackage(string packageName, byte[] content);
}

public sealed class SchedulerService : ISchedulerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IScheduler _scheduler;
    private readonly IPackageStorage _packageStorage;
    private readonly IPackageRunStorage _packageRunStorage;
    private readonly DisposableBagBuilder _subscriberDisposableBagBuilder;

    public SchedulerService(IServiceProvider serviceProvider, IScheduler scheduler, IPackageStorage packageStorage, IPackageRunStorage packageRunStorage)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _packageStorage = packageStorage ?? throw new ArgumentNullException(nameof(packageStorage));
        _packageRunStorage = packageRunStorage ?? throw new ArgumentNullException(nameof(packageRunStorage));
        _subscriberDisposableBagBuilder = DisposableBag.CreateBuilder();
    }

    public Task Start(ILoggerFactory loggerFactory)
    {
        LoggerFactory.SetLoggerFactory(loggerFactory);

        return _scheduler.Start();
    }

    public Task Shutdown()
    {
        _subscriberDisposableBagBuilder.Build().Dispose();

        return _scheduler.Shutdown();
    }

    public TService GetService<TService>()
        where TService : notnull
    {
        return _serviceProvider.GetRequiredService<TService>();
    }

    public void SubscribeToEvent<TEvent>(IAsyncMessageHandler<TEvent> handler)
    {
        var subscriber = _serviceProvider.GetRequiredService<IAsyncSubscriber<TEvent>>();
        subscriber.Subscribe(handler).AddTo(_subscriberDisposableBagBuilder);
    }

    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        var jobHistoryDelegate = _scheduler.GetJobHistoryDelegate();
        var jobExecutionDetail = await jobHistoryDelegate.GetJobExecutionDetail(id);
        if (jobExecutionDetail == null)
        {
            return null;
        }

        var logs = await jobHistoryDelegate.GetJobRunLogs(id);

        return new JobExecutionDetailModel(jobExecutionDetail, logs, _packageRunStorage.GetAttachmentNames(jobExecutionDetail.PackageName, id));
    }

    public Task<string?> GetJobExecutionErrorDetail(string id)
    {
        return _scheduler.GetJobHistoryDelegate().GetJobExecutionErrorDetail(id);
    }

    public Task<PagedResult<JobExecutionHistory>> GetJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        return _scheduler.GetJobHistoryDelegate().GetJobExecutionHistory(jobName, pageSize, page);
    }

    public async Task AddOrUpdateJob(JobAddOrUpdateModel jobModel)
    {
        var job = JobBuilder.Create()
            .OfType<ConsoleAppPackageJob>()
            .WithIdentity(jobModel.JobName, jobModel.JobGroup)
            .WithDescription(jobModel.Description)
            .UsingJobData("package", jobModel.Package)
            .UsingJobData("parameters", jobModel.Parameters)
            .Build();
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobModel.JobGroup}.{jobModel.JobName}", $"{jobModel.JobGroup}.{jobModel.JobName}")
            .WithCronSchedule(jobModel.CronExpression)
            .ForJob(jobModel.JobName, jobModel.JobGroup)
            .Build();

        var jobKey = new JobKey(jobModel.JobName, jobModel.JobGroup);
        if (await _scheduler.CheckExists(jobKey))
        {
            await _scheduler.DeleteJob(jobKey);
        }

        await _scheduler.ScheduleJob(job, trigger);
    }

    public async Task<JobDetailModel?> GetJobDetail(JobKey jobKey)
    {
        var jobDetail = await _scheduler.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        var cronExpression = await GetCronExpression(jobKey);

        return new JobDetailModel
                   {
                       JobName = jobDetail.Key.Name,
                       JobGroup = jobDetail.Key.Group,
                       Description = jobDetail.Description,
                       CronExpression = cronExpression,
                       CronExpressionDescription = GetCronExpressionDescription(cronExpression),
                       Package = GetJobData(jobDetail, "package"),
                       Parameters = GetJobData(jobDetail, "parameters")
                   };
    }

    public IList<string> GetPackages()
    {
        return _packageStorage.GetPackages();
    }

    public PackageDetailsModel? GetPackageDetails(string packageName)
    {
        return _packageStorage.GetPackageDetails(packageName);
    }

    public async Task<IList<JobListItemModel>> GetJobList()
    {
        var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false);
        var result = new List<JobListItemModel>(jobKeys.Count);
        foreach (var jobKey in jobKeys)
        {
            var jobDetail = await _scheduler.GetJobDetail(jobKey).ConfigureAwait(false);
            if (jobDetail != null)
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobDetail.Key);
                var nextFireTime = triggers.Select(x => x.GetNextFireTimeUtc()?.UtcDateTime).Where(x => x != null).OrderBy(x => x).FirstOrDefault()?.ToLocalTime();
                var lastFireTime = triggers.Select(x => x.GetPreviousFireTimeUtc()?.UtcDateTime).Where(x => x != null).OrderByDescending(x => x).FirstOrDefault()?.ToLocalTime();

                result.Add(new JobListItemModel
                                   {
                                       JobName = jobDetail.Key.Name,
                                       JobGroup = jobDetail.Key.Group,
                                       JobType = jobDetail.JobType.Name,
                                       TriggerDescription = GetCronExpressionDescription(triggers),
                                       NextFireTime = nextFireTime,
                                       LastFireTime = lastFireTime
                                   });
            }
        }

        return result;
    }

    public async Task<(SchedulerMetaData, IReadOnlyCollection<SchedulerStateRecord>, JobExecutionStatistics)> GetStatistics()
    {
        return (
                   await _scheduler.GetMetaData().ConfigureAwait(false), 
                   await _scheduler.GetInstances().ConfigureAwait(false),
                   await _scheduler.GetJobHistoryDelegate().GetJobExecutionStatistics().ConfigureAwait(false)
                   );
    }

    public byte[]? GetAttachmentBytes(string packageName, string jobRunId, string attachmentName)
    {
        return _packageRunStorage.GetAttachmentBytes(packageName, jobRunId, attachmentName);
    }

    public Task SavePackage(string packageName, byte[] content)
    {
        return _packageStorage.SavePackage(packageName, content);
    }

    private static string? GetJobData(IJobDetail jobDetail, string key)
    {
        if (jobDetail.JobDataMap.TryGetValue(key, out var value))
        {
            return value as string;
        }

        return null;
    }

    private async Task<string?> GetCronExpression(JobKey jobKey)
    {
        var triggers = await _scheduler.GetTriggersOfJob(jobKey);
        return GetCronExpression(triggers);
    }

    private static string? GetCronExpression(IEnumerable<ITrigger> triggers)
    {
        var trigger = triggers.Single() as ICronTrigger;
        return trigger?.CronExpressionString;
    }

    private static string GetCronExpressionDescription(IEnumerable<ITrigger> triggers)
    {
        var cronExpression = GetCronExpression(triggers);
        return GetCronExpressionDescription(cronExpression);
    }

    private static string GetCronExpressionDescription(string? cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return string.Empty;
        }

        try
        {
            return CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression);
        }
        catch
        {
            return string.Empty;
        }
    }
}