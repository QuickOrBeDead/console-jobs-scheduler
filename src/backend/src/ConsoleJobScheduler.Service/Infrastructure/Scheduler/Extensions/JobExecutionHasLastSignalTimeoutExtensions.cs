namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Extensions;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public static class JobExecutionHasLastSignalTimeoutExtensions
{
    public static void UpdateHasSignalTimeout(this IJobExecutionHasLastSignalTimeout item)
    {
        TimeSpan timeout = TimeSpan.FromMinutes(1);
        if (item.Completed || item.Vetoed)
        {
            item.HasSignalTimeout = false;
        }
        else
        {
            item.HasSignalTimeout = DateTime.UtcNow.Subtract(item.LastSignalTime.ToUniversalTime()) > timeout;
        }
    }
}