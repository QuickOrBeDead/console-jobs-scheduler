namespace ConsoleJobScheduler.WindowsService.Controllers;

using ConsoleJobScheduler.WindowsService.Scheduler;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PackagesController : Controller
{
    private readonly ISchedulerService _schedulerService;

    public PackagesController(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService ?? throw new ArgumentNullException(nameof(schedulerService));
    }

    [HttpGet]
    public IList<string> Get()
    {
        return _schedulerService.GetPackages();
    }
}