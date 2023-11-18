namespace ConsoleJobScheduler.Service.Api.Models;

public sealed class LoginModel
{
    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; }
}