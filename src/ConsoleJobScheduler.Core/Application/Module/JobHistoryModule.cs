using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Application.Module;

public sealed class JobHistoryModule
{
    private readonly IConfigurationRoot _configuration;

    public JobHistoryModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services, Action<DbContextOptionsBuilder>? dbContextOptionsBuilderAction = null)
    {
        services.AddDbContext<HistoryDbContext>(o =>
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
        services.AddScoped<IJobHistoryRepository, JobHistoryRepository>();
        services.AddScoped<IJobHistoryService, JobHistoryService>();
        services.AddScoped<IJobHistoryApplicationService, JobHistoryApplicationService>();
    }

    public async Task MigrateDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await using var historyDbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        await historyDbContext.Database.MigrateAsync().ConfigureAwait(false);
    }
}