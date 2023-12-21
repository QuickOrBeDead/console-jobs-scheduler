namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using Data;
using ConsoleJobScheduler.Service.Infrastructure.Extensions;
using Jobs;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;
using Models;

using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Impl.Matchers;

using Settings;
using ConsoleJobScheduler.Service.Infrastructure.Settings.Service;

public interface ISchedulerService
{
    Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task<PagedResult<JobExecutionHistory>> ListJobExecutionHistory(string jobName = "", int page = 1);

    Task AddOrUpdateJob(JobAddOrUpdateModel jobModel);

    Task<JobDetailModel?> GetJobDetail(JobKey jobKey);

    Task<List<string>> ListPackageNames();

    Task<PackageDetailsModel?> GetPackageDetails(string packageName);

    Task<PagedResult<JobListItemModel>> ListJobs(int page = 1);

    Task<(SchedulerMetaData, IReadOnlyCollection<SchedulerStateRecord>, JobExecutionStatistics)> GetStatistics();

    Task<byte[]?> GetAttachmentBytes(long id);

    Task SavePackage(string packageName, byte[] content);

    Task<PagedResult<PackageListItemModel>> ListPackages(int page = 1);
    Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData();
}

public sealed class SchedulerService : ISchedulerService
{
    private readonly IScheduler _scheduler;
    private readonly ISettingsService _settingsService;

    public SchedulerService(IScheduler scheduler, ISettingsService settingsService)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        var jobStoreDelegate = _scheduler.GetJobStoreDelegate();
        var jobExecutionDetail = await jobStoreDelegate.GetJobExecutionDetail(id).ConfigureAwait(false);
        if (jobExecutionDetail == null)
        {
            return null;
        }

        jobExecutionDetail.CronExpressionDescription = await GetCronExpressionDescription(new JobKey(jobExecutionDetail.JobName, jobExecutionDetail.JobGroup)).ConfigureAwait(false);

        var logs = await jobStoreDelegate.GetJobRunLogs(id).ConfigureAwait(false);
        var attachments = await jobStoreDelegate.GetJobRunAttachments(id).ConfigureAwait(false);
        return new JobExecutionDetailModel(jobExecutionDetail, logs, attachments);
    }

    public Task<string?> GetJobExecutionErrorDetail(string id)
    {
        return _scheduler.GetJobStoreDelegate().GetJobExecutionErrorDetail(id);
    }

    public async Task<PagedResult<JobExecutionHistory>> ListJobExecutionHistory(string jobName = "", int page = 1)
    {
        return await _scheduler.GetJobStoreDelegate().ListJobExecutionHistory(jobName, await GetPageSize().ConfigureAwait(false), page).ConfigureAwait(false);
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

    public Task<List<string>> ListPackageNames()
    {
        return _scheduler.GetJobStoreDelegate().ListPackageNames();
    }

    public async Task<PagedResult<PackageListItemModel>> ListPackages(int page = 1)
    {
        return await _scheduler.GetJobStoreDelegate().ListPackages(await GetPageSize().ConfigureAwait(false), page).ConfigureAwait(false);
    }

    public Task<PackageDetailsModel?> GetPackageDetails(string packageName)
    {
        return _scheduler.GetJobStoreDelegate().GetPackageDetails(packageName);
    }

    public async Task<PagedResult<JobListItemModel>> ListJobs(int page = 1)
    {
        var allJobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false);
        var pageSize = await GetPageSize().ConfigureAwait(false);
        var totalCount = allJobKeys.Count;
        var jobKeys = allJobKeys.OrderBy(x => x.Name)
                                                 .ThenBy(x => x.Group)
                                                 .Skip((page - 1) * pageSize)
                                                 .Take(pageSize);
        var result = new List<JobListItemModel>();
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

        return new PagedResult<JobListItemModel>(result, pageSize, page, totalCount);
    }

    public async Task<(SchedulerMetaData, IReadOnlyCollection<SchedulerStateRecord>, JobExecutionStatistics)> GetStatistics()
    {
        return (
                   await _scheduler.GetMetaData().ConfigureAwait(false),
                   await _scheduler.GetInstances().ConfigureAwait(false),
                   await _scheduler.GetJobStoreDelegate().GetJobExecutionStatistics().ConfigureAwait(false)
                   );
    }

    public Task<byte[]?> GetAttachmentBytes(long id)
    {
        return _scheduler.GetJobStoreDelegate().GetJobRunAttachmentContent(id);
    }

    public Task SavePackage(string packageName, byte[] content)
    {
        return _scheduler.GetJobStoreDelegate().SavePackage(packageName, content);
    }

    public Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData()
    {
        return _scheduler.GetJobStoreDelegate().ListJobExecutionHistoryChartData();
    }

    private static string? GetJobData(IJobDetail jobDetail, string key)
    {
        if (jobDetail.JobDataMap.TryGetValue(key, out var value))
        {
            return value as string;
        }

        return null;
    }

    private async Task<string?> GetCronExpressionDescription(JobKey jobKey)
    {
        var cronExpression = await GetCronExpression(jobKey).ConfigureAwait(false);
        return GetCronExpressionDescription(cronExpression);
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

    private async Task<int> GetPageSize()
    {
        return (await _settingsService.GetSettings<GeneralSettings>().ConfigureAwait(false)).PageSize ?? 10;
    }
}