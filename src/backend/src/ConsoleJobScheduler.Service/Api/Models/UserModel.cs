namespace ConsoleJobScheduler.Service.Api.Models;

public sealed class UserModel
{
    private IList<string>? _roles;

    public string? UserName { get; set; }

    public IList<string> Roles
    {
        get => _roles ??= new List<string>();
        set => _roles = value;
    }
}