using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public interface IJobPackageRepository
{
    ValueTask<JobPackage?> Get(string name);

    Task Add(JobPackage jobPackage);

    Task<TValue?> FindAsNoTracking<TValue>(string name, Func<JobPackage, TValue> projectFunc);

    IQueryable<JobPackage> QueryableAsNoTracking();

    Task SaveChanges();
}

public sealed class JobPackageRepository : IJobPackageRepository
{
    private readonly RunnerDbContext _runnerDbContext;

    public JobPackageRepository(RunnerDbContext runnerDbContext)
    {
        _runnerDbContext = runnerDbContext;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public ValueTask<JobPackage?> Get(string name)
    {
        return _runnerDbContext.FindAsync<JobPackage>(name);
    }

    public Task Add(JobPackage jobPackage)
    {
        return _runnerDbContext.AddAsync(jobPackage).AsTask();
    }

    public Task<TValue?> FindAsNoTracking<TValue>(string name, Func<JobPackage, TValue> projectFunc)
    {
        return
            _runnerDbContext.JobPackages.AsNoTracking().Where(x => x.Name == name)
                .Select(x => projectFunc(x))
                .SingleOrDefaultAsync();
    }

    public IQueryable<JobPackage> QueryableAsNoTracking()
    {
        return _runnerDbContext.JobPackages.AsQueryable().AsNoTracking();
    }

    public Task SaveChanges()
    {
        return _runnerDbContext.SaveChangesAsync();
    }
}