namespace ConsoleJobScheduler.Service.Api.Controllers;

using Infrastructure.Scheduler;
using Models;

using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;

[Authorize]
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
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<SchedulerInfoModel> Get()
    {
        var (metaData, nodes, statistics) = await _schedulerService.GetStatistics();

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

    [HttpGet("GetJobHistoryChartData")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<List<JobHistoryChartDataModel>> ListJobExecutionHistoryChartData()
    {
        return (await _schedulerService.ListJobExecutionHistoryChartData().ConfigureAwait(false))
            .ConvertAll(x => new JobHistoryChartDataModel
                                        {
                                            X = x.Date,
                                            Y = x.Count
                                        });
    }
}