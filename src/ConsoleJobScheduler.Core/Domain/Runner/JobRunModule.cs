using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public sealed class JobRunModule
{
    private readonly IConfigurationRoot _configuration;

    public JobRunModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IJobRunAttachmentRepository, JobRunAttachmentRepository>();
        services.AddSingleton<IJobPackageRepository, JobPackageRepository>();
        services.AddSingleton<IJobRunRepository, JobRunRepository>();
        services.AddSingleton<IJobRunService, JobRunService>();

        var appRunTempRootPath = _configuration["ConsoleAppPackageRunTempPath"] ?? AppDomain.CurrentDomain.BaseDirectory;

        services.AddScoped<IConsoleAppPackageRunner>(x => new ConsoleAppPackageRunner(x.GetRequiredService<IJobRunService>(), appRunTempRootPath));
    }
}