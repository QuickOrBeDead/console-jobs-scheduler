using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Jobs;
using ConsoleJobScheduler.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Util;

namespace ConsoleJobScheduler.Core.Domain.Scheduler
{
    public sealed class SchedulerModule
    {
        private readonly IConfigurationRoot _configuration;

        public SchedulerModule(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public void Register(IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, ServiceProviderJobFactory>();
            services.AddTransient<ConsoleAppPackageJob>();

            services.AddSingleton<IScheduler>(x =>
            {
                var schedulerBuilder = SchedulerBuilder.Create()
                    .WithId(_configuration["SchedulerInstanceId"]!)
                    .WithName("ConsoleJobsSchedulerService")
                    .UseDefaultThreadPool(y => y.MaxConcurrency = 100)
                    .UseJobFactory<ServiceProviderJobFactory>()
                    .UsePersistentStore(
                        o =>
                        {
                            o.UseClustering();
                            o.UseProperties = true;
                            o.UseNewtonsoftJsonSerializer();
                            o.UsePostgres(
                                p =>
                                {
                                    p.TablePrefix = _configuration["TablePrefix"]!;
                                    p.ConnectionString = _configuration["ConnectionString"]!;
                                });
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
        }
    }
}