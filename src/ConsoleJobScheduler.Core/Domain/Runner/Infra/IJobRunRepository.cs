using ConsoleJobScheduler.Core.Domain.Runner.Model;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobRunRepository
{
    Task SaveJobRunLog(JobRunLog jobRunLog, CancellationToken cancellationToken = default);

    Task<List<JobRunLog>> GetJobRunLogs(string jobRunId);
}