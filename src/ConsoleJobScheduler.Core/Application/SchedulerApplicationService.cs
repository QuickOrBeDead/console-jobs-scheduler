using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz;

namespace ConsoleJobScheduler.Core.Application;

public interface ISchedulerApplicationService
{
    Task<SchedulerInfoModel> GetStatistics();

    Task<PagedResult<JobListItem>> ListJobs(int? pageNumber = null);

    Task<JobDetail?> GetJobDetail(string group, string name);

    Task AddOrUpdateJob(JobAddOrUpdateModel model);
}

public sealed class SchedulerApplicationService(ISchedulerService schedulerService) : ISchedulerApplicationService
{
    private readonly ISchedulerService _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));

    public async Task<SchedulerInfoModel> GetStatistics()
    {
        var (metaData, nodes) =  (
            await _schedulerService.GetMetaData().ConfigureAwait(false),
            await _schedulerService.GetInstances().ConfigureAwait(false)
        );

        return new SchedulerInfoModel(
            new SchedulerMetadataModel
            {
                InStandbyMode = metaData.InStandbyMode,
                JobStoreType = metaData.JobStoreType.Name,
                SchedulerInstanceId = metaData.SchedulerInstanceId,
                SchedulerName = metaData.SchedulerName,
                SchedulerRemote = metaData.SchedulerRemote,
                SchedulerType = metaData.SchedulerType.ToString(),
                Shutdown = metaData.Shutdown,
                Started = metaData.Started,
                Summary = metaData.GetSummary(),
                ThreadPoolSize = metaData.ThreadPoolSize,
                ThreadPoolType = metaData.ThreadPoolType.ToString(),
                Version = metaData.Version,
                RunningSince = metaData.RunningSince?.UtcDateTime,
                JobStoreClustered = metaData.JobStoreClustered,
                JobStoreSupportsPersistence = metaData.JobStoreSupportsPersistence
            },
            nodes.Select(
                x => new SchedulerStateRecordModel
                {
                    SchedulerInstanceId = x.SchedulerInstanceId,
                    CheckInInterval = x.CheckinInterval,
                    CheckInTimestamp = x.CheckinTimestamp
                }).ToList());
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
}