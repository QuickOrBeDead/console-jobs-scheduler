namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler.Models;
using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;
using Quartz;

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
    [Produces("application/json")]
    public async Task<IList<JobListItemModel>> Get()
    {
        return await _schedulerService.GetJobList();
    }

    [HttpGet("{group}/{name}")]
    [Produces("application/json")]
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
    public Task Post([FromBody] JobAddOrUpdateModel jobAddOrUpdateModel)
    {
        return _schedulerService.AddOrUpdateJob(jobAddOrUpdateModel);
    }
}