namespace ConsoleJobScheduler.WindowsService.Extensions;

using System.Diagnostics.CodeAnalysis;

using ConsoleJobScheduler.WindowsService.Factory;
using ConsoleJobScheduler.WindowsService.JobStore;
using ConsoleJobScheduler.WindowsService.Plugins;

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
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        scheduler.Context.Add(key, value);
    }

    public static void AddJobStore(this IScheduler scheduler, IExtendedJobStore? jobStore)
    {
        if (jobStore == null)
        {
            throw new ArgumentNullException(nameof(jobStore));
        }

        scheduler.AddToContext(CustomSchedulerFactory.JobStoreContextKey, jobStore);
    }

    public static void AddJobHistoryDelegate(this IScheduler scheduler, IJobHistoryDelegate? jobHistoryDelegate)
    {
        if (jobHistoryDelegate == null)
        {
            throw new ArgumentNullException(nameof(jobHistoryDelegate));
        }

        scheduler.AddToContext(JobExecutionHistoryPlugin.JobExecutionHistoryDelegateContextKey, jobHistoryDelegate);
    }

    public static IJobHistoryDelegate GetJobHistoryDelegate(this IScheduler scheduler)
    {
        return GetContextValue<IJobHistoryDelegate>(scheduler, JobExecutionHistoryPlugin.JobExecutionHistoryDelegateContextKey);
    }

    public static IExtendedJobStore GetJobStore(this IScheduler scheduler)
    {
        return GetContextValue<IExtendedJobStore>(scheduler, CustomSchedulerFactory.JobStoreContextKey);
    }

    public static TValue GetContextValue<TValue>(this IScheduler scheduler, string key)
        where TValue : class
    {
        if (!scheduler.Context.TryGetValue(key, out var jobStoreObject))
        {
            throw new InvalidOperationException($"{key} not found in scheduler context");
        }

        if (jobStoreObject is not TValue jobStore)
        {
            throw new InvalidOperationException($"{key} must be {typeof(TValue)}");
        }

        return jobStore;
    }
}