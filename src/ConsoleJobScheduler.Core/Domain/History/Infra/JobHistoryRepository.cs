using ConsoleJobScheduler.Core.Domain.History.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.History.Infra;

public interface IJobHistoryRepository
{
    Task Add(JobExecutionHistory history, CancellationToken cancellationToken = default);

    ValueTask<JobExecutionHistory?> FindExecutionHistory(string id, CancellationToken cancellationToken = default);

    Task<TValue?> FindExecutionHistory<TValue>(string id, Func<JobExecutionHistory, TValue> projectFunc);

    Task SaveChanges(CancellationToken cancellationToken = default);

    IQueryable<JobExecutionHistory> Queryable();
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

    public ValueTask<JobExecutionHistory?> FindExecutionHistory(string id, CancellationToken cancellationToken = default)
    {
        return _historyDbContext.Histories.FindAsync(id, cancellationToken);
    }

    public Task<TValue?> FindExecutionHistory<TValue>(string id, Func<JobExecutionHistory, TValue> projectFunc)
    {
        return
            _historyDbContext.Histories.Where(x => x.Id == id)
                .Select(x => projectFunc(x))
                .SingleOrDefaultAsync();
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