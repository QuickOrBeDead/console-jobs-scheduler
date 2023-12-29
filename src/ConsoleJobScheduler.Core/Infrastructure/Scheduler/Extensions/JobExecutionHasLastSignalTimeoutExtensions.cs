using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins.Models;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Extensions;

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