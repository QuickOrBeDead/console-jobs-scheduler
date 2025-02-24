using System.Net.Mime;
using ConsoleJobScheduler.Core.Api.Model;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Identity.Model;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class PackagesController : ControllerBase
{
    private readonly IJobApplicationService _jobApplicationService;

    public PackagesController(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService ?? throw new ArgumentNullException(nameof(jobApplicationService));
    }

    [HttpGet("GetPackageNames")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<List<string>> GetPackageNames()
    {
        return _jobApplicationService.GetAllPackageNames();
    }

    [HttpGet("List/{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<PagedResult<JobPackageListItem>> Get(int? pageNumber = null)
    {
        return _jobApplicationService.ListPackages(10, pageNumber ?? 1);
    }

    [HttpGet("Detail")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JobPackageDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail([FromQuery] string packageName)
    {
        var result = await _jobApplicationService.GetPackageDetails(packageName).ConfigureAwait(false);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("Save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Save([FromForm] PackageSaveModel model)
    {
        if (model.File == null || string.IsNullOrWhiteSpace(model.Name))
        {
            return BadRequest();
        }

        await using MemoryStream ms = new();
        await model.File.CopyToAsync(ms).ConfigureAwait(false);
        await _jobApplicationService.SavePackage(model.Name, ms.ToArray()).ConfigureAwait(false);
        return Ok();
    }
}