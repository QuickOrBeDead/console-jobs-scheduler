using ConsoleJobScheduler.Core.Domain.Scheduler;
using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Infra.Migration;
using ConsoleJobScheduler.Core.Jobs;
using ConsoleJobScheduler.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Util;

namespace ConsoleJobScheduler.Core.Application.Module;

public enum SchedulerDbType
{
    Postgresql = 0,
    SqLite = 1
}

public sealed class SchedulerModule(IConfigurationRoot configuration, SchedulerDbType schedulerDbType = SchedulerDbType.Postgresql)
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IJobFactory, ServiceProviderJobFactory>();
        services.AddTransient<ConsoleAppPackageJob>();

        services.AddSingleton<IScheduler>(x =>
        {
            var schedulerBuilder = SchedulerBuilder.Create()
                .WithId(configuration["SchedulerInstanceId"]!)
                .WithName(configuration["SchedulerInstanceName"] ?? "ConsoleJobsSchedulerService")
                .UseDefaultThreadPool(y => y.MaxConcurrency = 100)
                .UseJobFactory<ServiceProviderJobFactory>()
                .UsePersistentStore(
                    o =>
                    {
                        if (schedulerDbType != SchedulerDbType.SqLite)
                        {
                            o.UseClustering();
                        }

                        o.UseProperties = true;
                        o.UseNewtonsoftJsonSerializer();

                        Action<SchedulerBuilder.AdoProviderOptions> dbConfigurator = p =>
                        {
                            p.TablePrefix = configuration["TablePrefix"]!;
                            p.ConnectionString = configuration["ConnectionString"]!;
                        };

                        if (schedulerDbType == SchedulerDbType.Postgresql)
                        {
                            o.UsePostgres(dbConfigurator);
                        }
                        else
                        {
                            o.UseMicrosoftSQLite(dbConfigurator);
                        }
                    });
            schedulerBuilder.SetProperty(StdSchedulerFactory.PropertyJobStoreType, typeof(CustomJobStoreTx).AssemblyQualifiedNameWithoutVersion());
            schedulerBuilder.SetProperty(JobExecutionHistoryPlugin.PluginConfigurationProperty, typeof(JobExecutionHistoryPlugin).AssemblyQualifiedNameWithoutVersion());

            var factory = new CustomSchedulerFactory(x, schedulerBuilder.Properties);
            return factory.GetScheduler().Result;
        });
        services.AddSingleton<IQuartzDbStore>(x => x.GetRequiredService<IScheduler>().GetJobStore());
        services.AddScoped<ISchedulerService, SchedulerService>();
        services.AddSingleton<ISchedulerManager, SchedulerManager>();
        services.AddSingleton<JobExecutionHistoryPlugin>();
        services.AddScoped<ISchedulerApplicationService, SchedulerApplicationService>();
    }

    public void MigrateDb()
    {
        var dbMigrationRunner = new DbMigrationRunner();
        dbMigrationRunner.Migrate(configuration["ConnectionString"]!, configuration["TablePrefix"]!, "Scheduler", GetDbType());
    }

    private string GetDbType()
    {
        return schedulerDbType switch
        {
            SchedulerDbType.Postgresql => "PostgreSQL",
            SchedulerDbType.SqLite => "Sqlite",
            _ => "PostgreSQL"
        };
    }
}