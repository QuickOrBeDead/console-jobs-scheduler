using ConsoleJobScheduler.Core.Domain.Runner.Extensions;
using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Domain.Settings;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Jobs;
using CronExpressionDescriptor;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;

namespace ConsoleJobScheduler.Core.Domain.Scheduler;

public interface ISchedulerService
{
    Task AddOrUpdateJob(JobAddOrUpdateModel jobModel);

    Task<JobDetail?> GetJobDetail(JobKey jobKey);

    Task<PagedResult<JobListItem>> ListJobs(int page = 1);

    Task<ITrigger?> GetTrigger(TriggerKey triggerKey);

    Task<SchedulerMetaData> GetMetaData();

    Task<IReadOnlyCollection<SchedulerStateRecord>> GetInstances();

    Task<JobCronExpression?> GetCronExpression(JobKey jobKey);
}

public sealed class SchedulerService : ISchedulerService
{
    private readonly IScheduler _scheduler;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SchedulerService> _logger;

    public SchedulerService(IScheduler scheduler, ISettingsService settingsService, ILogger<SchedulerService> logger)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public async Task<JobDetail?> GetJobDetail(JobKey jobKey)
    {
        var jobDetail = await _scheduler.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        var cronExpression = await GetCronExpression(jobKey).ConfigureAwait(false);

        return new JobDetail
        {
            JobName = jobDetail.Key.Name,
            JobGroup = jobDetail.Key.Group,
            Description = jobDetail.Description,
            CronExpression = cronExpression?.Expression,
            CronExpressionDescription = cronExpression?.Description,
            Package = jobDetail.GetPackageName(),
            Parameters = jobDetail.GetParameters()
        };
    }

    public async Task<PagedResult<JobListItem>> ListJobs(int page = 1)
    {
        var allJobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false);
        var pageSize = await GetPageSize().ConfigureAwait(false);
        var totalCount = allJobKeys.Count;
        var jobKeys = allJobKeys.OrderBy(x => x.Name)
                                                 .ThenBy(x => x.Group)
                                                 .Skip((page - 1) * pageSize)
                                                 .Take(pageSize);
        var result = new List<JobListItem>();
        foreach (var jobKey in jobKeys)
        {
            var jobDetail = await _scheduler.GetJobDetail(jobKey).ConfigureAwait(false);
            if (jobDetail != null)
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobDetail.Key);
                var nextFireTime = triggers.Select(x => x.GetNextFireTimeUtc()?.UtcDateTime).Where(x => x != null).OrderBy(x => x).FirstOrDefault()?.ToLocalTime();
                var lastFireTime = triggers.Select(x => x.GetPreviousFireTimeUtc()?.UtcDateTime).Where(x => x != null).OrderByDescending(x => x).FirstOrDefault()?.ToLocalTime();

                result.Add(new JobListItem
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

        return new PagedResult<JobListItem>(result, pageSize, page, totalCount);
    }

    public Task<ITrigger?> GetTrigger(TriggerKey triggerKey)
    {
        return _scheduler.GetTrigger(triggerKey);
    }

    public Task<SchedulerMetaData> GetMetaData()
    {
        return _scheduler.GetMetaData();
    }

    public Task<IReadOnlyCollection<SchedulerStateRecord>> GetInstances()
    {
        return _scheduler.GetJobStore().SelectSchedulerStateRecords();
    }

    public async Task<JobCronExpression?> GetCronExpression(JobKey jobKey)
    {
        var triggers = await _scheduler.GetTriggersOfJob(jobKey).ConfigureAwait(false);
        var cronExpression = GetCronExpression(triggers);
        return string.IsNullOrWhiteSpace(cronExpression) ? null : new JobCronExpression(cronExpression, GetCronExpressionDescription(cronExpression));
    }

    private async Task<int> GetPageSize()
    {
        var generalSettings = await _settingsService.GetSettings<GeneralSettings>().ConfigureAwait(false);
        return generalSettings.PageSize ?? 10;
    }

    private static string? GetCronExpression(IEnumerable<ITrigger> triggers)
    {
        var trigger = triggers.SingleOrDefault() as ICronTrigger;
        return trigger?.CronExpressionString;
    }

    private string GetCronExpressionDescription(IEnumerable<ITrigger> triggers)
    {
        var cronExpression = GetCronExpression(triggers);
        return GetCronExpressionDescription(cronExpression);
    }

    private string GetCronExpressionDescription(string? cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return string.Empty;
        }

        try
        {
            return ExpressionDescriptor.GetDescription(cronExpression);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Cron expression '{cronExpression}' get description error.", cronExpression);
            return string.Empty;
        }
    }
}