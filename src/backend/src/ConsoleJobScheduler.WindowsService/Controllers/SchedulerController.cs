namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;

using ConsoleJobScheduler.WindowsService.Models;

[Route("api/[controller]")]
[ApiController]
public sealed class SchedulerController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public SchedulerController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet]
    [Produces("application/json")]
    public async Task<SchedulerInfoModel> Get()
    {
        var (metaData, nodes, statistics) = await _schedulerService.GetStatistics();

        return new SchedulerInfoModel(
            new SchedulerMetadataModel
                {
                    InStandbyMode = metaData.InStandbyMode,
                    JobStoreType = metaData.JobStoreType.ToString(),
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
}