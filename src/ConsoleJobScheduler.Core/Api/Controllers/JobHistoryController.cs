using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public sealed class JobHistoryController : ControllerBase
{
    private readonly IJobHistoryApplicationService _jobHistoryApplicationService;

    public JobHistoryController(IJobHistoryApplicationService jobHistoryApplicationService)
    {
        _jobHistoryApplicationService = jobHistoryApplicationService ?? throw new ArgumentNullException(nameof(jobHistoryApplicationService));
    }

    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<JobExecutionHistoryListItem>))]
    public Task<PagedResult<JobExecutionHistoryListItem>> Get([FromQuery] string jobName = "", int? pageNumber = null)
    {
        return _jobHistoryApplicationService.ListJobExecutionHistory(jobName, page: pageNumber ?? 1);
    }

    [HttpGet("GetJobHistoryChartData")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData()
    {
        return _jobHistoryApplicationService.ListJobExecutionHistoryChartData();
    }

    [HttpGet("GetJobExecutionStatistics")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<JobExecutionStatistics> GetJobExecutionStatistics()
    {
        return _jobHistoryApplicationService.GetJobExecutionStatistics();
    }

    [HttpGet("GetJobExecutionDetail/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobExecutionHistoryDetail))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobExecutionDetail(string id)
    {
        var jobExecutionDetail = await _jobHistoryApplicationService.GetJobExecutionDetail(id).ConfigureAwait(false);
        if (jobExecutionDetail == null)
        {
            return NotFound();
        }

        return Ok(jobExecutionDetail);
    }

    [HttpGet("GetErrorDetail/{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<string?> GetErrorDetail(string id)
    {
        return _jobHistoryApplicationService.GetJobExecutionErrorDetail(id);
    }
}