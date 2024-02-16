namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class UserDetailModel
{
    private IList<string>? _roles;

    public string? UserName { get; set; }

    public IList<string> Roles
    {
        get
        {
            _roles ??= new List<string>();

            return _roles;
        }
        set => _roles = value;
    }
}