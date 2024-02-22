using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler;

namespace ConsoleJobScheduler.Core.Application;

public interface ISchedulerApplicationService
{
    Task<SchedulerInfoModel> GetStatistics();

    Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData();
}

public sealed class SchedulerApplicationService : ISchedulerApplicationService
{
    private readonly ISchedulerService _schedulerService;
    private readonly IJobHistoryService _jobHistoryService;

    public SchedulerApplicationService(
        ISchedulerService schedulerService,
        IJobHistoryService jobHistoryService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        _jobHistoryService = jobHistoryService ?? throw new ArgumentNullException(nameof(jobHistoryService));
    }

    public async Task<SchedulerInfoModel> GetStatistics()
    {
        var (metaData, nodes, statistics) =  (
            await _schedulerService.GetMetaData().ConfigureAwait(false),
            await _schedulerService.GetInstances().ConfigureAwait(false),
            await _jobHistoryService.GetJobExecutionStatistics().ConfigureAwait(false)
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
                }).ToList(),
            new SchedulerJobExecutionStatisticsModel
            {
                TotalExecutedJobs = statistics.TotalExecutedJobs,
                TotalFailedJobs = statistics.TotalFailedJobs,
                TotalRunningJobs = statistics.TotalRunningJobs,
                TotalSucceededJobs = statistics.TotalSucceededJobs,
                TotalVetoedJobs = statistics.TotalVetoedJobs
            });
    }

    public Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData()
    {
        return _jobHistoryService.ListJobExecutionHistoryChartData();
    }
}