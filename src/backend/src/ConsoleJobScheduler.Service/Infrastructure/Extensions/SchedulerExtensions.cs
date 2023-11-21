namespace ConsoleJobScheduler.Service.Infrastructure.Extensions;

using Scheduler.Plugins;
using Scheduler;

using System.Diagnostics.CodeAnalysis;

using Quartz;
using Quartz.Impl.AdoJobStore;

public static class SchedulerExtensions
{
    public static Task<IReadOnlyCollection<SchedulerStateRecord>> GetInstances(this IScheduler scheduler)
    {
        var jobStore = GetJobStore(scheduler);
        return jobStore.SelectSchedulerStateRecords();
    }

    public static void AddToContext<TValue>(this IScheduler scheduler, string key, [DisallowNull] TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        scheduler.Context.Add(key, value);
    }

    public static void AddJobStore(this IScheduler scheduler, IExtendedJobStore? jobStore)
    {
        ArgumentNullException.ThrowIfNull(jobStore);

        scheduler.AddToContext(CustomSchedulerFactory.JobStoreContextKey, jobStore);
    }

    public static void AddJobStoreDelegate(this IScheduler scheduler, IJobStoreDelegate? jobStoreDelegate)
    {
        ArgumentNullException.ThrowIfNull(jobStoreDelegate);

        scheduler.AddToContext(JobStoreDelegate.JobStoreDelegateContextKey, jobStoreDelegate);
    }

    public static IJobStoreDelegate GetJobStoreDelegate(this IScheduler scheduler)
    {
        return GetContextValue<IJobStoreDelegate>(scheduler, JobStoreDelegate.JobStoreDelegateContextKey);
    }

    public static IExtendedJobStore GetJobStore(this IScheduler scheduler)
    {
        return GetContextValue<IExtendedJobStore>(scheduler, CustomSchedulerFactory.JobStoreContextKey);
    }

    public static TValue GetContextValue<TValue>(this IScheduler scheduler, string key)
        where TValue : class
    {
        if (!scheduler.Context.TryGetValue(key, out var valueObject))
        {
            throw new InvalidOperationException($"{key} not found in scheduler context");
        }

        if (valueObject is not TValue value)
        {
            throw new InvalidOperationException($"{key} must be {typeof(TValue)}");
        }

        return value;
    }
}