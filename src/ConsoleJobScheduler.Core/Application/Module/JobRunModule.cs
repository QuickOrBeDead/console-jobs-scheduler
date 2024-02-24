using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Application.Module;

public sealed class JobRunModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IJobRunAttachmentRepository, JobRunAttachmentRepository>();
        services.AddSingleton<IJobPackageRepository, JobPackageRepository>();
        services.AddSingleton<IJobRunRepository, JobRunRepository>();
        services.AddScoped<IJobRunService, JobRunService>();
        services.AddScoped<IConsoleAppPackageRunner, ConsoleAppPackageRunner>();
        services.AddScoped<IConsoleMessageProcessor, EmailConsoleMessageProcessor>();
        services.AddScoped<IConsoleMessageProcessor, ConsoleLogMessageProcessor>();
        services.AddScoped<IConsoleMessageProcessorManager, ConsoleMessageProcessorManager>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
    }
}