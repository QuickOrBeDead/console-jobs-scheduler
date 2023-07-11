namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Data;
using ConsoleJobScheduler.WindowsService.Plugins.Model;
using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

[Route("api/[controller]")]
[ApiController]
public sealed class JobHistoryController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public JobHistoryController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<JobExecutionHistory>))]
    public Task<PagedResult<JobExecutionHistory>> Get(int? pageNumber = null)
    {
        return _schedulerService.GetJobExecutionHistory(page: pageNumber.GetValueOrDefault(1));
    }
}