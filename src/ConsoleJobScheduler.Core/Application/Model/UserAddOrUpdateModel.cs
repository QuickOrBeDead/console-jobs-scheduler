using System.ComponentModel.DataAnnotations;

namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class UserAddOrUpdateModel
{
    private IList<string>? _roles;

    public int Id { get; set; }

    [Display(Name = "Username")]
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