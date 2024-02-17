using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz;

namespace ConsoleJobScheduler.Core.Application;

public interface IJobApplicationService
{
    Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id);
    Task<string?> GetJobExecutionErrorDetail(string id);
    Task<byte[]?> GetJobRunAttachmentContent(long id);
    Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);
    Task SavePackage(string packageName, byte[] content);
    Task<List<string>> GetAllPackageNames();
    Task<PackageDetails?> GetPackageDetails(string name);
    Task<PagedResult<PackageListItem>> ListPackages(int pageSize = 10, int page = 1);
    Task InsertJobHistoryEntry(IJobExecutionContext context, CancellationToken cancellationToken = default);
    Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, JobExecutionException? jobException, CancellationToken cancellationToken = default);
    Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default);
    Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default);
    Task<PagedResult<JobListItem>> ListJobs(int? pageNumber = null);
    Task<JobDetail?> GetJobDetail(string group, string name);
    Task AddOrUpdateJob(JobAddOrUpdateModel model);
}

public sealed class JobApplicationService : IJobApplicationService
{
    private readonly IJobHistoryRepository _jobHistoryRepository;
    private readonly IJobHistoryService _jobHistoryService;
    private readonly IJobRunService _jobRunService;
    private readonly ISchedulerService _schedulerService;
    private readonly IJobRunRepository _jobRunRepository;
    private readonly IJobRunAttachmentRepository _jobRunAttachmentRepository;
    private readonly IJobPackageRepository _jobPackageRepository;

    public JobApplicationService(
        IJobHistoryRepository jobHistoryRepository,
        IJobRunRepository jobRunRepository,
        IJobRunAttachmentRepository jobRunAttachmentRepository,
        IJobPackageRepository jobPackageRepository,
        IJobHistoryService jobHistoryService,
        IJobRunService jobRunService,
        ISchedulerService schedulerService)
    {
        _jobHistoryRepository = jobHistoryRepository;
        _jobHistoryService = jobHistoryService;
        _jobRunService = jobRunService;
        _schedulerService = schedulerService;
        _jobRunRepository = jobRunRepository;
        _jobRunAttachmentRepository = jobRunAttachmentRepository;
        _jobPackageRepository = jobPackageRepository;
    }

    public Task InsertJobHistoryEntry(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var jobExecutionHistory = new JobExecutionHistory(
            context.FireInstanceId,
            context.Scheduler.SchedulerName,
            context.Scheduler.SchedulerInstanceId,
            GetJobData(context.JobDetail, "package"),
            context.JobDetail.Key.Name,
            context.JobDetail.Key.Group,
            context.Trigger.Key.Name,
            context.Trigger.Key.Group,
            context.ScheduledFireTimeUtc,
            context.FireTimeUtc,
            context.ScheduledFireTimeUtc);
        return _jobHistoryRepository.InsertJobHistoryEntry(jobExecutionHistory, cancellationToken);
    }

    public async Task<JobExecutionDetailModel?> GetJobExecutionDetail(string id)
    {
        var jobExecutionDetail = await _jobHistoryService.GetJobExecutionDetail(id).ConfigureAwait(false);
        if (jobExecutionDetail == null)
        {
            return null;
        }

        var cronExpression = await _schedulerService.GetCronExpression(new JobKey(jobExecutionDetail.JobName, jobExecutionDetail.JobGroup)).ConfigureAwait(false);
        jobExecutionDetail.CronExpressionDescription = cronExpression?.Description;

        var logs = await _jobRunRepository.GetJobRunLogs(id).ConfigureAwait(false);
        var attachments = await _jobRunAttachmentRepository.GetJobRunAttachments(id).ConfigureAwait(false);
        return new JobExecutionDetailModel(jobExecutionDetail, logs, attachments);
    }

    public Task<string?> GetJobExecutionErrorDetail(string id)
    {
        return _jobHistoryRepository.GetJobExecutionErrorDetail(id);
    }

    public Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        return _jobRunAttachmentRepository.GetJobRunAttachmentContent(id);
    }

    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        var result = await _jobHistoryService.ListJobExecutionHistory(jobName, pageSize, page);
        var triggers = new Dictionary<TriggerKey, ITrigger?>();

        foreach (var jobExecutionHistory in result.Items)
        {
            var triggerKey = new TriggerKey(jobExecutionHistory.TriggerName, jobExecutionHistory.TriggerGroup);
            if (!triggers.TryGetValue(triggerKey, out var trigger))
            {
                triggers[triggerKey] = trigger = await _schedulerService.GetTrigger(triggerKey);
            }

            if (trigger != null)
            {
                jobExecutionHistory.NextFireTime = trigger.GetFireTimeAfter(jobExecutionHistory.ScheduledTime)?.DateTime.ToLocalTime();
            }
        }

        return result;
    }

    public Task SavePackage(string packageName, byte[] content)
    {
        return _jobRunService.SavePackage(packageName, content);
    }

    public Task<List<string>> GetAllPackageNames()
    {
        return _jobPackageRepository.GetAllPackageNames();
    }

    public Task<PackageDetails?> GetPackageDetails(string name)
    {
        return _jobPackageRepository.GetPackageDetails(name);
    }

    public Task<PagedResult<PackageListItem>> ListPackages(int pageSize = 10, int page = 1)
    {
        return _jobPackageRepository.ListPackages(pageSize, page);
    }

    public Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        return _jobHistoryRepository.UpdateJobHistoryEntryCompleted(id, runTime, jobException, cancellationToken);
    }

    public Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default)
    {
        return _jobHistoryRepository.UpdateJobHistoryEntryVetoed(id, cancellationToken);
    }

    public Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default)
    {
        return _jobHistoryRepository.UpdateJobHistoryEntryLastSignalTime(id, signalTime, cancellationToken);
    }

    public Task<PagedResult<JobListItem>> ListJobs(int? pageNumber = null)
    {
        return _schedulerService.ListJobs(pageNumber ?? 1);
    }

    public async Task<JobDetail?> GetJobDetail(string group, string name)
    {
        var jobKey = new JobKey(name, group);
        var jobDetail = await _schedulerService.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return null;
        }

        return
            new JobDetail
            {
                JobName = jobDetail.JobName,
                JobGroup = jobDetail.JobGroup,
                Description = jobDetail.Description,
                CronExpression = jobDetail.CronExpression,
                CronExpressionDescription = jobDetail.CronExpressionDescription,
                Package = jobDetail.Package,
                Parameters = jobDetail.Parameters
            };
    }

    public Task AddOrUpdateJob(JobAddOrUpdateModel model)
    {
        return _schedulerService.AddOrUpdateJob(model);
    }

    private static string? GetJobData(IJobDetail jobDetail, string key)
    {
        if (jobDetail.JobDataMap.TryGetValue(key, out var value))
        {
            return value as string;
        }

        return null;
    }
}