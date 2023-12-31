﻿using System.Net.Mime;

using ConsoleJobScheduler.Core.Api.Models;
using ConsoleJobScheduler.Core.Infrastructure.Data;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Quartz;

namespace ConsoleJobScheduler.Core.Api.Controllers;

using JobDetailModel = Infrastructure.Scheduler.Models.JobDetailModel;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class JobsController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public JobsController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.JobEditor},{Roles.JobViewer}")]
    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<PagedResult<JobListItemModel>> Get(int? pageNumber = null)
    {
        return _schedulerService.ListJobs(pageNumber ?? 1);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.JobEditor}")]
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

        await _schedulerService.AddOrUpdateJob(model).ConfigureAwait(false);

        return Ok();
    }
}