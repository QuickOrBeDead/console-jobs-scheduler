namespace ConsoleJobScheduler.Service.Infrastructure.Settings;

using System.ComponentModel.DataAnnotations;

using ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

public sealed class SmtpSettings : ISettings
{
    public string Host { get; set; } = default!;

    public int Port { get; set; }

    public string From { get; set; } = default!;

    [Display(Name = "From Name")]
    public string FromName { get; set; } = default!;

    [Display(Name = "Enable Ssl")]
    public bool EnableSsl { get; set; }

    [Display(Name = "Username")]
    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;

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