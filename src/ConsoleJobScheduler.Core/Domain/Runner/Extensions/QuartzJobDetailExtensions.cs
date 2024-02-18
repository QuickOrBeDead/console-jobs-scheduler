using Quartz;

namespace ConsoleJobScheduler.Core.Domain.Runner.Extensions;

public static class QuartzJobDetailExtensions
{
    public static string? GetPackageName(this IJobDetail jobDetail)
    {
        return GetJobData(jobDetail, "package");
    }

    public static string? GetParameters(this IJobDetail jobDetail)
    {
        return GetJobData(jobDetail, "parameters");
    }

    private static string? GetJobData(IJobDetail jobDetail, string key)
    {
        if (jobDetail.JobDataMap.TryGetValue(key, out var value))
        {
            return value as string;
        }

        return null;
    }
}