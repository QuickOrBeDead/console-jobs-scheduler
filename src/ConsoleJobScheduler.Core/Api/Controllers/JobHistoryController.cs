using System.Net.Mime;

using ConsoleJobScheduler.Core.Infrastructure.Data;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

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
    public Task<PagedResult<JobExecutionHistory>> Get([FromQuery] string jobName = "", int? pageNumber = null)
    {
        return _schedulerService.ListJobExecutionHistory(jobName, page: pageNumber ?? 1);
    }
}