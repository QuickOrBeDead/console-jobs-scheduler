using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Identity.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class JobsController : ControllerBase
{
    private readonly ISchedulerApplicationService _schedulerApplicationService;

    public JobsController(ISchedulerApplicationService schedulerApplicationService)
    {
        _schedulerApplicationService = schedulerApplicationService ?? throw new ArgumentNullException(nameof(schedulerApplicationService));
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.JobEditor},{Roles.JobViewer}")]
    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<PagedResult<JobListItem>> Get(int? pageNumber = null)
    {
        return _schedulerApplicationService.ListJobs(pageNumber ?? 1);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.JobEditor}")]
    [HttpGet("{group}/{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobDetail))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string group, string name)
    {
        var jobDetail = await _schedulerApplicationService.GetJobDetail(group, name);
        if (jobDetail == null)
        {
            return NotFound();
        }

        return Ok(jobDetail);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.JobEditor}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] JobAddOrUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.JobName) || string.IsNullOrWhiteSpace(model.JobGroup))
        {
            return BadRequest();
        }

        await _schedulerApplicationService.AddOrUpdateJob(model).ConfigureAwait(false);

        return Ok();
    }
}