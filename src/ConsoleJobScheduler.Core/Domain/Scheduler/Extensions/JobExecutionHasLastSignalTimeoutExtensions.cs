using ConsoleJobScheduler.Core.Domain.History.Model;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;

public static class JobExecutionHasLastSignalTimeoutExtensions
{
    public static void UpdateHasSignalTimeout(this IJobExecutionHasLastSignalTimeout item, DateTimeOffset now)
    {
        var timeout = TimeSpan.FromMinutes(1);
        if (item.Completed || item.Vetoed)
        {
            item.SetHasSignalTimeout(false);
        }
        else
        {
            item.SetHasSignalTimeout(now.Subtract(item.LastSignalTime.ToUniversalTime()) > timeout);
        }
    }
}