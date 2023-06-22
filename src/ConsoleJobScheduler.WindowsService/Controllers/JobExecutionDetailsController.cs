namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler;

using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class JobExecutionDetailsController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public JobExecutionDetailsController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    public async Task<IActionResult> Get(string id)
    {
       var jobDetail = await _schedulerService.GetJobExecutionDetail(id).ConfigureAwait(false);
       if (jobDetail == null)
       {
           return NotFound();
       }

       return Ok(jobDetail);
    }

    [HttpGet("GetErrorDetail/{id}")]
    [Produces("application/json")]
    public Task<string?> GetErrorDetail(string id)
    {
        return _schedulerService.GetJobExecutionErrorDetail(id);
    }

    [HttpGet("GetAttachment/{id}")]
    [Produces("application/octet-stream")]
    public IActionResult GetAttachment(string id, [FromQuery]string packageName, [FromQuery]string attachmentName)
    {
        var fileContents = _schedulerService.GetAttachmentBytes(packageName, id, attachmentName);
        if (fileContents == null)
        {
            return NotFound();
        }

        return File(fileContents, "application/octet-stream", attachmentName);
    }
}