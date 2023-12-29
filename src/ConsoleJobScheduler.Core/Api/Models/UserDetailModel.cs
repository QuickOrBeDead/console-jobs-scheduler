namespace ConsoleJobScheduler.Core.Api.Models;

public sealed class UserDetailModel
{
    private IList<string?>? _roles;

    public string? UserName { get; set; }

    public List<string?> Roles
    {
        get
        {
            _roles ??= new List<string?>();

            return (List<string?>)_roles;
        }
        set => _roles = value;
    }
}