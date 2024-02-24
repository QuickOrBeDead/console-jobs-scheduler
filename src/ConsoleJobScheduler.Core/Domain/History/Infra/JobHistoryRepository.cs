using ConsoleJobScheduler.Core.Domain.History.Model;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Domain.History.Infra;

public interface IJobHistoryRepository
{
    Task Add(JobExecutionHistory history, CancellationToken cancellationToken = default);

    Task<TValue?> FindExecutionHistoryAsNoTracking<TValue>(string id, Func<JobExecutionHistory, TValue> projectFunc);

    Task SaveChanges(CancellationToken cancellationToken = default);

    IQueryable<JobExecutionHistory> Queryable();

    Task SetLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default);

    Task SetVetoed(string id, CancellationToken cancellationToken = default);

    Task SetCompleted(string id, TimeSpan runTime, CancellationToken cancellationToken = default);

    Task SetCompleted(string id, TimeSpan runTime, string errorMessage, string errorDetails, CancellationToken cancellationToken = default);
}

public sealed class JobHistoryRepository : IJobHistoryRepository
{
    private readonly HistoryDbContext _historyDbContext;

    public JobHistoryRepository(HistoryDbContext historyDbContext)
    {
        _historyDbContext = historyDbContext;
    }

    public Task Add(JobExecutionHistory history, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.AddAsync(history, cancellationToken).AsTask();
    }

    public Task<TValue?> FindExecutionHistoryAsNoTracking<TValue>(string id, Func<JobExecutionHistory, TValue> projectFunc)
    {
        return
            _historyDbContext.Histories.AsNoTracking().Where(x => x.Id == id)
                .Select(x => projectFunc(x))
                .SingleOrDefaultAsync();
    }

    public Task SetLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.Histories
            .Where(x => x.Id == id)
            .UpdateAsync(_ => new JobExecutionHistory { LastSignalTime = signalTime }, cancellationToken);
    }

    public Task SetVetoed(string id, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.Histories
            .Where(x => x.Id == id)
            .UpdateAsync(_ => new JobExecutionHistory { Vetoed = true }, cancellationToken);
    }

    public Task SetCompleted(string id, TimeSpan runTime, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.Histories
            .Where(x => x.Id == id)
            .UpdateAsync(_ => new JobExecutionHistory
            {
                Completed = true,
                RunTime = runTime
            }, cancellationToken);
    }

    public Task SetCompleted(string id, TimeSpan runTime, string errorMessage, string errorDetails, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.Histories
            .Where(x => x.Id == id)
            .UpdateAsync(_ => new JobExecutionHistory
            {
                Completed = true,
                RunTime = runTime,
                HasError = true,
                ErrorMessage = errorMessage,
                ErrorDetails = errorDetails
            }, cancellationToken);
    }

    public IQueryable<JobExecutionHistory> Queryable()
    {
        return _historyDbContext.Histories;
    }

    public Task SaveChanges(CancellationToken cancellationToken = default)
    {
        return _historyDbContext.SaveChangesAsync(cancellationToken);
    }
}