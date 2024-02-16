using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsApplicationService _settingsApplicationService;

    public SettingsController(ISettingsApplicationService settingsService)
    {
        _settingsApplicationService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    [HttpGet(nameof(GetGeneralSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<GeneralSettings> GetGeneralSettings()
    {
        return _settingsApplicationService.GetSettings<GeneralSettings>();
    }

    [HttpPost(nameof(SaveGeneralSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public Task SaveGeneralSettings([FromBody] GeneralSettings settings)
    {
        return _settingsApplicationService.SaveSettings(settings);
    }

    [HttpGet(nameof(GetSmtpSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<SmtpSettings> GetSmtpSettings()
    {
        return _settingsApplicationService.GetSettings<SmtpSettings>();
    }

    [HttpPost(nameof(SaveSmtpSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public Task SaveSmtpSettings([FromBody] SmtpSettings settings)
    {
        return _settingsApplicationService.SaveSettings(settings);
    }
}