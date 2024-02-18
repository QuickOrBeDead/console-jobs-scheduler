using ConsoleJobScheduler.Core.Domain.History.Infra;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.History;

public sealed class JobHistoryModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IJobHistoryRepository, JobHistoryRepository>();
        services.AddSingleton<IJobHistoryService, JobHistoryService>();
    }
}