namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class UserListItem
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Roles { get; set; }
}