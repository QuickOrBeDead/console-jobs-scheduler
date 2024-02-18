namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class LoginModel
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}