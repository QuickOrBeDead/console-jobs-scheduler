using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler;

namespace ConsoleJobScheduler.Core.Application;

public interface ISchedulerApplicationService
{
    Task<SchedulerInfoModel> GetStatistics();
}

public sealed class SchedulerApplicationService : ISchedulerApplicationService
{
    private readonly ISchedulerService _schedulerService;

    public SchedulerApplicationService(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

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
}