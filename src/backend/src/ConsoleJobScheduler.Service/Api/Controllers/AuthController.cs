namespace ConsoleJobScheduler.Service.Api.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

using Models;

using Microsoft.AspNetCore.Authorization;

using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using System.Security.Claims;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser<int>> _signInManager;

    public AuthController(SignInManager<IdentityUser<int>> signInManager)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    [HttpPost("Login")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task<SignInResult> Login(LoginModel loginModel)
    {
        return _signInManager.PasswordSignInAsync(loginModel.UserName, loginModel.Password, loginModel.RememberMe, true);
    }

    [HttpPost("Logout")]
    [Produces(MediaTypeNames.Application.Json)]
    public Task Logout()
    {
        return _signInManager.SignOutAsync();
    }

    [HttpGet("GetUser")]
    [Produces(MediaTypeNames.Application.Json)]
    public UserModel? GetUser()
    {
        var contextUser = HttpContext.User;
        if (!_signInManager.IsSignedIn(contextUser))
        {
            return null;
        }

        return new UserModel
        {
            UserName = GetClaimValue(contextUser, ClaimTypes.Name),
            Roles = GetRoles(contextUser)
        };
    }

    private static string? GetClaimValue(ClaimsPrincipal contextUser, string type)
    {
        return contextUser.Claims.FirstOrDefault(x => x.Type == type)?.Value;
    }

    private static List<string> GetRoles(ClaimsPrincipal contextUser)
    {
        return contextUser.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
    }
}