using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class SchedulerController : ControllerBase
{
    private readonly ISchedulerApplicationService _schedulerService;

    public SchedulerController(ISchedulerApplicationService schedulerApplicationService)
    {
        _schedulerService = schedulerApplicationService ?? throw new ArgumentNullException(nameof(schedulerApplicationService));
    }

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<SchedulerInfoModel> Get()
    {
        return _schedulerService.GetStatistics();
    }

    [HttpGet("GetJobHistoryChartData")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<List<JobHistoryChartDataModel>> ListJobExecutionHistoryChartData()
    {
        return _schedulerService.ListJobExecutionHistoryChartData();
    }
}