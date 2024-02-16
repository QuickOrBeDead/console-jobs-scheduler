using ConsoleJobScheduler.Core.Domain.History.Model;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;

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