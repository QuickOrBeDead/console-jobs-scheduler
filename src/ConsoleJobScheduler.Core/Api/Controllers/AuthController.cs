using System.Net.Mime;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ConsoleJobScheduler.Core.Api.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IIdentityApplicationService _identityApplicationService;

    public AuthController(IIdentityApplicationService identityApplicationService)
    {
        _identityApplicationService = identityApplicationService;
    }

    [HttpPost("Login")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<SignInResult> Login(LoginModel loginModel)
    {
        return _identityApplicationService.Login(loginModel);
    }

    [HttpPost("Logout")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task Logout()
    {
        return _identityApplicationService.Logout();
    }

    [HttpGet("GetUser")]
    [Produces(MediaTypeNames.Application.Json)]
    public UserContext? GetUser()
    {
        return _identityApplicationService.GetUserContext(HttpContext.User);
    }
}