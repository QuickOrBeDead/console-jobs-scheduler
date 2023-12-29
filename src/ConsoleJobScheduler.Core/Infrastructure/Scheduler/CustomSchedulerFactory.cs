using System.Collections.Specialized;

using ConsoleJobScheduler.Core.Infrastructure.Extensions;

using Microsoft.Extensions.DependencyInjection;

using Quartz;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Util;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler;

public sealed class CustomSchedulerFactory : StdSchedulerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public const string JobStoreContextKey = "quartz.JobStore";

    public CustomSchedulerFactory(IServiceProvider serviceProvider, NameValueCollection props)
        : base(props)
    {
        _serviceProvider = serviceProvider;
    }

    protected override IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
    {
        IScheduler scheduler = new StdScheduler(qs);
        scheduler.AddJobStore(rsrcs.JobStore as IExtendedJobStore);
        return scheduler;
    }

    protected override T InstantiateType<T>(Type? implementationType)
    {
        return _serviceProvider.GetService<T>() ?? ObjectUtils.InstantiateType<T>(implementationType);
    }
}