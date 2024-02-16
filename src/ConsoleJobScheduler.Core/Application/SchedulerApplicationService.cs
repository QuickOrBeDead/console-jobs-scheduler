using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.Scheduler;

namespace ConsoleJobScheduler.Core.Application;

public interface ISchedulerApplicationService
{
    Task<SchedulerInfoModel> GetStatistics();

    Task<List<JobHistoryChartDataModel>> ListJobExecutionHistoryChartData();
}

public sealed class SchedulerApplicationService : ISchedulerApplicationService
{
    private readonly ISchedulerService _schedulerService;
    private readonly IJobHistoryRepository _jobHistoryRepository;

    public SchedulerApplicationService(ISchedulerService schedulerService, IJobHistoryRepository jobHistoryRepository)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
        _jobHistoryRepository = jobHistoryRepository ?? throw new ArgumentNullException(nameof(jobHistoryRepository));
    }

    public async Task<SchedulerInfoModel> GetStatistics()
    {
        var (metaData, nodes, statistics) =  (
            await _schedulerService.GetMetaData().ConfigureAwait(false),
            await _schedulerService.GetInstances().ConfigureAwait(false),
            await _jobHistoryRepository.GetJobExecutionStatistics().ConfigureAwait(false)
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

    public async Task<List<JobHistoryChartDataModel>> ListJobExecutionHistoryChartData()
    {
        return (await _jobHistoryRepository.ListJobExecutionHistoryChartData().ConfigureAwait(false))
            .ConvertAll(x => new JobHistoryChartDataModel
            {
                X = x.Date,
                Y = x.Count
            });
    }
}