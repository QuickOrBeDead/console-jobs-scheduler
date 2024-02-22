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
    private readonly IJobHistoryApplicationService _jobApplicationService;

    public JobHistoryController(IJobHistoryApplicationService jobHistoryApplicationService)
    {
        _jobApplicationService = jobHistoryApplicationService ?? throw new ArgumentNullException(nameof(jobHistoryApplicationService));
    }

    [HttpGet("{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<JobExecutionHistoryListItem>))]
    public Task<PagedResult<JobExecutionHistoryListItem>> Get([FromQuery] string jobName = "", int? pageNumber = null)
    {
        return _jobApplicationService.ListJobExecutionHistory(jobName, page: pageNumber ?? 1);
    }
}