using ConsoleJobScheduler.Core.Domain.Runner.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobRunRepository
{
    Task Add(JobRunLog jobRunLog, CancellationToken cancellationToken = default);

    IQueryable<JobRunLog> QueryableAsNoTracking();

    Task SaveChanges(CancellationToken cancellationToken = default);
}

public sealed class JobRunRepository : IJobRunRepository
{
    private readonly RunnerDbContext _runnerDbContext;

    public JobRunRepository(RunnerDbContext runnerDbContext)
    {
        _runnerDbContext = runnerDbContext;
    }

    public Task Add(JobRunLog jobRunLog, CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.AddAsync(jobRunLog, cancellationToken).AsTask();
    }

    public IQueryable<JobRunLog> QueryableAsNoTracking()
    {
        return _runnerDbContext.JobRunLogs.AsQueryable().AsNoTracking();
    }

    public Task SaveChanges(CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.SaveChangesAsync(cancellationToken);
    }
}