﻿using System.Net.Mime;

using ConsoleJobScheduler.Core.Infrastructure.Scheduler;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

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
    public async Task<IActionResult> GetAttachment(long id, [FromQuery] string attachmentName)
    {
        var fileContents = await _schedulerService.GetAttachmentBytes(id).ConfigureAwait(false);
        if (fileContents == null)
        {
            return NotFound();
        }

        return File(fileContents, MediaTypeNames.Application.Octet, attachmentName);
    }
}