namespace ConsoleJobScheduler.Core.Domain.History.Model;

public interface IJobExecutionHasLastSignalTimeout
{
    bool Completed { get; }

    bool Vetoed { get; }

    DateTime LastSignalTime { get; }

    bool HasSignalTimeout { get; set; }
}