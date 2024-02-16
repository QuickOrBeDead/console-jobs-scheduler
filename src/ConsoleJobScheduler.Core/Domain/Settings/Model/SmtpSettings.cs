using System.ComponentModel.DataAnnotations;

namespace ConsoleJobScheduler.Core.Domain.Settings.Model;

public sealed class SmtpSettings : ISettings
{
    [Required]
    public string Host { get; set; } = null!;

    [Required, Range(1, 65535)]
    public int? Port { get; set; }

    public string From { get; set; } = null!;

    [Display(Name = "From Name")]
    public string FromName { get; set; } = null!;

    [Display(Name = "Enable Ssl")]
    public bool EnableSsl { get; set; }

    [Display(Name = "Username")]
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Domain { get; set; }

    public SettingCategory GetCategory() => SettingCategory.Smtp;

    public void Map(SettingsData data)
    {
        Host = data.GetString(nameof(Host), "localhost");
        Port = data.GetInt(nameof(Port), 25);
        From = data.GetString(nameof(From), "console.jobs.scheduler@mail.com");
        FromName = data.GetString(nameof(FromName), "Console Jobs Scheduler");
        EnableSsl = data.GetBool(nameof(EnableSsl), false);
        UserName = data.GetString(nameof(UserName), string.Empty);
        Password = data.GetString(nameof(Password), string.Empty);
        Domain = data.GetString(nameof(Domain), null!);
    }

    public SettingsData GetData()
    {
        var result = new SettingsData();
        result.Set(nameof(Host), Host);
        result.Set(nameof(Port), Port);
        result.Set(nameof(From), From);
        result.Set(nameof(FromName), FromName);
        result.Set(nameof(EnableSsl), EnableSsl);
        result.Set(nameof(UserName), UserName);
        result.Set(nameof(Password), Password);
        result.Set(nameof(Domain), string.IsNullOrWhiteSpace(Domain) ? null : Domain);

        return result;
    }
}