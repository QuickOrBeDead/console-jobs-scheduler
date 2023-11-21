namespace ConsoleJobScheduler.Service.Api.Controllers;

using Models;
using ConsoleJobScheduler.Service.Infrastructure.Settings.Service;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using Infrastructure.Settings;

[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
[ApiController]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    [HttpGet(nameof(GetGeneralSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<GeneralSettings> GetGeneralSettings()
    {
        return _settingsService.GetSettings<GeneralSettings>();
    }

    [HttpPost(nameof(SaveGeneralSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public Task SaveGeneralSettings([FromBody] GeneralSettings settings)
    {
        return _settingsService.SaveSettings(settings);
    }

    [HttpGet(nameof(GetSmtpSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<SmtpSettings> GetSmtpSettings()
    {
        return _settingsService.GetSettings<SmtpSettings>();
    }

    [HttpPost(nameof(SaveSmtpSettings))]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public Task SaveSmtpSettings([FromBody] SmtpSettings settings)
    {
        return _settingsService.SaveSettings(settings);
    }
}