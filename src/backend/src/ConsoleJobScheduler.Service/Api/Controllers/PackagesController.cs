namespace ConsoleJobScheduler.Service.Api.Controllers;

using Infrastructure.Scheduler;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Models;
using Microsoft.AspNetCore.Mvc;

using System.Net.Mime;

using Models;

using Microsoft.AspNetCore.Authorization;
using Infrastructure.Data;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class PackagesController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public PackagesController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet("GetPackageNames")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<List<string>> GetPackageNames()
    {
        return _schedulerService.ListPackageNames();
    }

    [HttpGet("List/{pageNumber:int?}")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<PagedResult<PackageListItemModel>> Get(int? pageNumber = null)
    {
        return _schedulerService.ListPackages(pageNumber ?? 1);
    }

    [HttpGet("Detail")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PackageDetailsModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail([FromQuery] string packageName)
    {
        var result = await _schedulerService.GetPackageDetails(packageName).ConfigureAwait(false);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
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

        using MemoryStream ms = new();
        await model.File.CopyToAsync(ms).ConfigureAwait(false);
        await _schedulerService.SavePackage(model.Name, ms.ToArray()).ConfigureAwait(false);
        return Ok();
    }
}