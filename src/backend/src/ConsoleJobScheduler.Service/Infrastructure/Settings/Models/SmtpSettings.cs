namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Models;

using System.Globalization;

public sealed class SmtpSettings : ISettings
{
    public string Host { get; set; } = default!;

    public int Port { get; set; }

    public string From { get; set; } = default!;

    public string FromName { get; set; } = default!;

    public bool EnableSsl { get; set; }

    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;

    public string? Domain { get; set; }

    public int CategoryId => 1;

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

    public IDictionary<string, string?> GetData()
    {
        return new Dictionary<string, string?>
                   {
                       {nameof(Host), Host},
                       {nameof(Port), Port.ToString(CultureInfo.InvariantCulture)},
                       {nameof(From), From},
                       {nameof(FromName), FromName},
                       {nameof(EnableSsl), EnableSsl.ToString(CultureInfo.InvariantCulture)},
                       {nameof(UserName), UserName},
                       {nameof(Password), Password},
                       {nameof(Domain), Domain}
                   };
    }
}