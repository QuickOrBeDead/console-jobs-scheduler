namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

public interface IJobExecutionHasLastSignalTimeout
{
    bool Completed { get; }

    bool Vetoed { get; }

    DateTime LastSignalTime { get; }

    bool HasSignalTimeout { get; set; }
}