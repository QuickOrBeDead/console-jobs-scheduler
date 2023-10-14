namespace ConsoleJobScheduler.Service.Api.Models;

using System.ComponentModel.DataAnnotations;

public sealed class UserAddOrUpdateModel
{
    private IList<string>? _roles;

    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string? UserName { get; set; }

    [MaxLength(8)]
    public string? Password { get; set; }

    public IList<string> Roles
    {
        get => _roles ??= new List<string>();
        set => _roles = value;
    }
}