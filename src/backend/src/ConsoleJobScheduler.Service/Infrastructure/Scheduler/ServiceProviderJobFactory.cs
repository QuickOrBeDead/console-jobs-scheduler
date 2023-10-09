namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

public sealed class ServiceProviderJobFactory : PropertySettingJobFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var job = (IJob?)_serviceProvider.GetService(bundle.JobDetail.JobType);

        return job ?? base.NewJob(bundle, scheduler);
    }
}