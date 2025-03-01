using ConsoleJobScheduler.Core.Infra.Data;
using Quartz.Impl.AdoJobStore;
using Quartz.Spi;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;

public interface IExtendedJobStore : IJobStore
{
    Task<IReadOnlyCollection<SchedulerStateRecord>> SelectSchedulerStateRecords(CancellationToken cancellationToken = default);
}

public sealed class CustomJobStoreTx : JobStoreTX, IExtendedJobStore
{
    public Task<IReadOnlyCollection<SchedulerStateRecord>> SelectSchedulerStateRecords(CancellationToken cancellationToken = default)
    {
        return ExecuteWithoutLock(conn => Delegate.SelectSchedulerStateRecords(conn, null, cancellationToken), cancellationToken);
    }
}