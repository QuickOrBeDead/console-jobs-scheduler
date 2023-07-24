namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using ConsoleJobScheduler.WindowsService.Jobs.Models;

[Route("api/[controller]")]
[ApiController]
public sealed class PackagesController : ControllerBase
{
    private readonly ISchedulerService _schedulerService;

    public PackagesController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    public IList<string> Get()
    {
        return _schedulerService.GetPackages();
    }

    [HttpGet("Detail")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PackageDetailsModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromQuery]string packageName)
    {
        var result = _schedulerService.GetPackageDetails(packageName);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}