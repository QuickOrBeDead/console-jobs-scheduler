namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Events;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins;
using ConsoleJobScheduler.Service.Infrastructure.Settings.Data;
using ConsoleJobScheduler.Service.Infrastructure.Settings.Service;

using MessagePipe;

using Microsoft.EntityFrameworkCore;

using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Util;

public interface ISchedulerServiceBuilder
{
    Task<ISchedulerService> Build();
}

public sealed class SchedulerServiceBuilder : ISchedulerServiceBuilder
{
    private readonly IConfiguration _config;

    public SchedulerServiceBuilder(IConfiguration config)
    {
        _config = config;
    }

    public async Task<ISchedulerService> Build()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISchedulerFactory>(x =>
            {
                var schedulerBuilder = SchedulerBuilder.Create()
                    .WithId(_config["SchedulerInstanceId"])
                    .WithName("ConsoleJobsSchedulerService")
                    .UseDefaultThreadPool(y => y.MaxConcurrency = 100)
                    .UseJobFactory<ServiceProviderJobFactory>()
                    .UsePersistentStore(
                        o =>
                            {
                                o.UseClustering();
                                o.UseProperties = true;
                                o.UseJsonSerializer();
                                o.UsePostgres(
                                    p =>
                                        {
                                            p.TablePrefix = _config["TablePrefix"];
                                            p.ConnectionString = _config["ConnectionString"];
                                        });
                            });
                schedulerBuilder.SetProperty(StdSchedulerFactory.PropertyJobStoreType, typeof(CustomJobStoreTx).AssemblyQualifiedNameWithoutVersion());
                schedulerBuilder.SetProperty(JobExecutionHistoryPlugin.PluginConfigurationProperty, typeof(JobExecutionHistoryPlugin).AssemblyQualifiedNameWithoutVersion());

                return new CustomSchedulerFactory(x, schedulerBuilder.Properties);
            });
        services.AddSingleton<IJobFactory, ServiceProviderJobFactory>();

        var packageStorageRootPath = _config["ConsoleAppPackageStoragePath"] ?? AppDomain.CurrentDomain.BaseDirectory;
        var appRunTempRootPath = _config["ConsoleAppPackageRunTempPath"] ?? AppDomain.CurrentDomain.BaseDirectory;

        services.AddDbContext<SettingsDbContext>(o => o.UseNpgsql(_config["ConnectionString"]));

        services.AddSingleton<IPackageStorage>(_ => new DefaultPackageStorage(packageStorageRootPath));
        services.AddSingleton<IConsoleAppPackageRunner>(x => new DefaultConsoleAppPackageRunner(
            x.GetRequiredService<IAsyncPublisher<JobConsoleLogMessageEvent>>(),
            x.GetRequiredService<IPackageStorage>(),
            x.GetRequiredService<IEmailSender>(),
            appRunTempRootPath));
        services.AddTransient<ISettingsService, SettingsService>();
        services.AddTransient<IEmailSender>(x => new SmtpEmailSender(x.GetRequiredService<ISettingsService>()));
        services.AddTransient<ConsoleAppPackageJob>();

        services.AddMessagePipe(
            x =>
                {
                    x.InstanceLifetime = InstanceLifetime.Singleton;
                    x.RequestHandlerLifetime = InstanceLifetime.Singleton;
                    x.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
                    x.EnableAutoRegistration = false;
                });

        var serviceProvider = services.BuildServiceProvider();
        var scheduler = await serviceProvider.GetRequiredService<ISchedulerFactory>().GetScheduler();

        return new SchedulerService(serviceProvider, scheduler, serviceProvider.GetRequiredService<IPackageStorage>());
    }
}