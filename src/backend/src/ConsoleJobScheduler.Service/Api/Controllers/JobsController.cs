namespace ConsoleJobScheduler.Service.Api.Controllers;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using Microsoft.AspNetCore.Mvc;

using Quartz;

using System.Net.Mime;

[Route("api/[controller]")]
[ApiController]
public sealed class JobsController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public JobsController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<IList<JobListItemModel>> Get()
    {
        return await _schedulerService.GetJobList();
    }

    [HttpGet("{group}/{name}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string group, string name)
    {
        var jobKey = new JobKey(name, group);
        var jobDetail = await _schedulerService.GetJobDetail(jobKey);
        if (jobDetail == null)
        {
            return NotFound();
        }

        return Ok(
            new JobDetailModel
                {
                    JobName = jobDetail.JobName,
                    JobGroup = jobDetail.JobGroup,
                    Description = jobDetail.Description,
                    CronExpression = jobDetail.CronExpression,
                    CronExpressionDescription = jobDetail.CronExpressionDescription,
                    Package = jobDetail.Package,
                    Parameters = jobDetail.Parameters
                });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] JobAddOrUpdateModel model)
    {
        if (string.IsNullOrWhiteSpace(model.JobName) || string.IsNullOrWhiteSpace(model.JobGroup))
        {
            return BadRequest();
        }

        await _schedulerService.AddOrUpdateJob(model).ConfigureAwait(false);

        return Ok();
    }
}