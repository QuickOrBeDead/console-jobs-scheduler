using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;
using Quartz;
using Quartz.Impl.AdoJobStore;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;

public static class SchedulerExtensions
{
    private const string JobStoreContextKey = "quartz.JobStore";

    public static void AddJobStore(this IScheduler scheduler, IExtendedJobStore? jobStore)
    {
        ArgumentNullException.ThrowIfNull(jobStore);

        scheduler.AddToContext(JobStoreContextKey, jobStore);
    }

    public static IExtendedJobStore GetJobStore(this IScheduler scheduler)
    {
        return GetContextValue<IExtendedJobStore>(scheduler, JobStoreContextKey);
    }

    private static void AddToContext<TValue>(this IScheduler scheduler, string key, [DisallowNull] TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        scheduler.Context.Add(key, value);
    }

    private static TValue GetContextValue<TValue>(this IScheduler scheduler, string key)
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