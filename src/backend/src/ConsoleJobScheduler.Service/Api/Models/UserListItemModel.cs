namespace ConsoleJobScheduler.Service.Api.Models;

public sealed class UserListItemModel
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Roles { get; set; }
}