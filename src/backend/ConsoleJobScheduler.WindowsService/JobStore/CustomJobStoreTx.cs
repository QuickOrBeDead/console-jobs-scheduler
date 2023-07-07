namespace ConsoleJobScheduler.WindowsService.JobStore;

using Quartz.Impl.AdoJobStore;
using Quartz.Spi;

public interface IExtendedJobStore : IJobStore
{
    string DataSource { get; }

    string TablePrefix { get; }

    Task<IReadOnlyCollection<SchedulerStateRecord>> SelectSchedulerStateRecords(CancellationToken cancellationToken = default);

    IDbAccessor GetDbAccessor();

}

public sealed class CustomJobStoreTx : JobStoreTX, IExtendedJobStore
{
    public Task<IReadOnlyCollection<SchedulerStateRecord>> SelectSchedulerStateRecords(CancellationToken cancellationToken = default)
    {
        return ExecuteWithoutLock(conn => Delegate.SelectSchedulerStateRecords(conn, null, cancellationToken), cancellationToken);
    }

    public IDbAccessor GetDbAccessor()
    {
        if (Delegate is not IDbAccessor dbAccessor)
        {
            throw new InvalidOperationException($"Delegate must be {typeof(IDbAccessor)}");
        }

        return dbAccessor;
    }
}