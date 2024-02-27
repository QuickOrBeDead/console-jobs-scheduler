using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Application.Module;

public sealed class JobRunModule
{
    private readonly IConfigurationRoot _configuration;

    public JobRunModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services, Action<DbContextOptionsBuilder>? dbContextOptionsBuilderAction = null)
    {
        services.AddDbContext<RunnerDbContext>(o =>
        {
            if (dbContextOptionsBuilderAction == null)
            {
                o.UseNpgsql(_configuration["ConnectionString"]);
            }
            else
            {
                dbContextOptionsBuilderAction(o);
            }
        });
        services.AddScoped<IJobRunAttachmentRepository, JobRunAttachmentRepository>();
        services.AddScoped<IJobPackageRepository, JobPackageRepository>();
        services.AddScoped<IJobRunRepository, JobRunRepository>();
        services.AddScoped<IJobRunService, JobRunService>();
        services.AddScoped<IConsoleAppPackageRunner, ConsoleAppPackageRunner>();
        services.AddScoped<IConsoleMessageProcessor, EmailConsoleMessageProcessor>();
        services.AddScoped<IConsoleMessageProcessor, ConsoleLogMessageProcessor>();
        services.AddScoped<IConsoleMessageProcessorManager, ConsoleMessageProcessorManager>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
    }

    public async Task MigrateDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await using var runnerDbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        await runnerDbContext.Database.MigrateAsync().ConfigureAwait(false);
    }
}