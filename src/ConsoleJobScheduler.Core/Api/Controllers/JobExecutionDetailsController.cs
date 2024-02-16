using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class JobExecutionDetailsController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public JobExecutionDetailsController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService ?? throw new ArgumentNullException(nameof(jobApplicationService));
    }

    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobExecutionDetailModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string id)
    {
        var jobDetail = await _jobApplicationService.GetJobExecutionDetail(id).ConfigureAwait(false);
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
        return _jobApplicationService.GetJobExecutionErrorDetail(id);
    }

    [HttpGet("GetAttachment/{id}")]
    [Produces(MediaTypeNames.Application.Octet)]
    public async Task<IActionResult> GetAttachment(long id, [FromQuery] string attachmentName)
    {
        var fileContents = await _jobApplicationService.GetJobRunAttachmentContent(id).ConfigureAwait(false);
        if (fileContents == null)
        {
            return NotFound();
        }

        return File(fileContents, MediaTypeNames.Application.Octet, attachmentName);
    }
}