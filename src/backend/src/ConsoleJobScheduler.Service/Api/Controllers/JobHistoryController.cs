namespace ConsoleJobScheduler.Service.Api.Controllers;

using ConsoleJobScheduler.Service.Infrastructure.Data;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;

[Authorize]
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
    public Task<PagedResult<JobExecutionHistory>> Get([FromQuery]string jobName = "", int? pageNumber = null)
    {
        return _schedulerService.GetJobExecutionHistory(jobName, page: pageNumber.GetValueOrDefault(1));
    }
}