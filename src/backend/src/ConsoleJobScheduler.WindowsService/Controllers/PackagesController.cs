namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

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
}