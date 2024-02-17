using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public sealed class JobRunModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IJobRunAttachmentRepository, JobRunAttachmentRepository>();
        services.AddSingleton<IJobPackageRepository, JobPackageRepository>();
        services.AddSingleton<IJobRunRepository, JobRunRepository>();
        services.AddScoped<IJobRunService, JobRunService>();
        services.AddScoped<IConsoleAppPackageRunner, ConsoleAppPackageRunner>();
    }
}