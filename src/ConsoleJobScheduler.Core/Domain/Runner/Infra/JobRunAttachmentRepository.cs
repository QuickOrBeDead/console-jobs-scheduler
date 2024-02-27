using ConsoleJobScheduler.Core.Domain.Runner.Model;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobRunAttachmentRepository
{
    Task Add(JobRunEmail email, CancellationToken cancellationToken = default);

    Task Add(JobRunAttachment jobRunAttachment, CancellationToken cancellationToken = default);

    Task<TValue?> FindAsNoTracking<TValue>(long id, Func<JobRunAttachment, TValue> projectFunc);

    IQueryable<JobRunAttachment> QueryableAsNoTracking();

    Task SaveChanges(CancellationToken cancellationToken = default);

    Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default);
}

public sealed class JobRunAttachmentRepository : IJobRunAttachmentRepository
{
    private readonly RunnerDbContext _runnerDbContext;

    public JobRunAttachmentRepository(RunnerDbContext runnerDbContext)
    {
        _runnerDbContext = runnerDbContext;
    }

    public Task Add(JobRunEmail email, CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.AddAsync(email, cancellationToken).AsTask();
    }

    public Task Add(JobRunAttachment jobRunAttachment, CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.AddAsync(jobRunAttachment, cancellationToken).AsTask();
    }

    public Task<TValue?> FindAsNoTracking<TValue>(long id, Func<JobRunAttachment, TValue> projectFunc)
    {
        return _runnerDbContext.JobRunAttachments.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => projectFunc(x))
            .SingleOrDefaultAsync();
    }

    public IQueryable<JobRunAttachment> QueryableAsNoTracking()
    {
        return _runnerDbContext.JobRunAttachments.AsNoTracking();
    }

    public Task SaveChanges(CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default)
    {
        return _runnerDbContext.JobRunEmails.Where(x => x.Id == id)
            .UpdateAsync(_ => new JobRunEmail { IsSent = isSent }, cancellationToken);
    }
}