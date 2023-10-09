namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using Quartz;

[PersistJobDataAfterExecution]
public sealed class ConsoleAppPackageJob : IJob
{
    private readonly IConsoleAppPackageRunner _consoleAppPackageRunner;

    public ConsoleAppPackageJob(IConsoleAppPackageRunner consoleAppPackageRunner)
    {
        _consoleAppPackageRunner = consoleAppPackageRunner ?? throw new ArgumentNullException(nameof(consoleAppPackageRunner));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var jobData = context.JobDetail.JobDataMap;
        var package = jobData.GetString("package");
        if (package == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"package\") is null");
        }

        var parameters = jobData.GetString("parameters");
        if (parameters == null)
        {
            throw new InvalidOperationException("jobData.GetString(\"parameters\") is null");
        }

        await _consoleAppPackageRunner.Run(context.FireInstanceId, package, parameters, context.CancellationToken).ConfigureAwait(false);
    }
}