namespace ConsoleJobScheduler.Service.Api.Models;

public sealed class LoginModel
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}