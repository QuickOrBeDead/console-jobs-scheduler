namespace ConsoleJobScheduler.Core.Api.Models;

public sealed class UserListItemModel
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Roles { get; set; }
}