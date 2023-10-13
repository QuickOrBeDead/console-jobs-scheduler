namespace ConsoleJobScheduler.Service.Api.Controllers;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Models;

using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using Microsoft.AspNetCore.Authorization;

[Authorize]
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
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobExecutionDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [Produces(MediaTypeNames.Application.Json)]
    public Task<string?> GetErrorDetail(string id)
    {
        return _schedulerService.GetJobExecutionErrorDetail(id);
    }

    [HttpGet("GetAttachment/{id}")]
    [Produces(MediaTypeNames.Application.Octet)]
    public IActionResult GetAttachment(string id, [FromQuery]string packageName, [FromQuery]string attachmentName)
    {
        var fileContents = _schedulerService.GetAttachmentBytes(packageName, id, attachmentName);
        if (fileContents == null)
        {
            return NotFound();
        }

        return File(fileContents, MediaTypeNames.Application.Octet, attachmentName);
    }
}