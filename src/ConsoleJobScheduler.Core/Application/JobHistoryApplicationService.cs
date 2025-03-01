using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Infra.Data;

namespace ConsoleJobScheduler.Core.Application;

public interface IJobHistoryApplicationService
{
    Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

    Task InsertJobHistoryEntry(JobExecutionHistory jobExecutionHistory, CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, Exception? jobException, CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task<JobExecutionStatistics> GetJobExecutionStatistics();

    Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData();

    Task<JobExecutionHistoryDetail?> GetJobExecutionDetail(string id);
}

public sealed class JobHistoryApplicationService(
    IJobHistoryRepository jobHistoryRepository,
    IJobHistoryService jobHistoryService)
    : IJobHistoryApplicationService
{
    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await jobHistoryService.ListJobExecutionHistory(jobName, pageSize, page).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task InsertJobHistoryEntry(JobExecutionHistory jobExecutionHistory, CancellationToken cancellationToken = default)
    {
        await jobHistoryRepository.Add(jobExecutionHistory, cancellationToken).ConfigureAwait(false);
        await jobHistoryRepository.SaveChanges(cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, Exception? jobException, CancellationToken cancellationToken = default)
    {
        return jobHistoryService.UpdateJobHistoryEntryCompleted(id, runTime, jobException, cancellationToken);
    }

    public Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default)
    {
        return jobHistoryRepository.SetVetoed(id, cancellationToken);
    }

    public Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default)
    {
        return jobHistoryRepository.SetLastSignalTime(id, signalTime, cancellationToken);
    }

    public async Task<string?> GetJobExecutionErrorDetail(string id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await jobHistoryService.GetJobExecutionErrorDetail(id).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<JobExecutionHistoryDetail?> GetJobExecutionDetail(string id)
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  jobHistoryService.GetJobExecutionDetail(id).ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<JobExecutionStatistics> GetJobExecutionStatistics()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await jobHistoryService.GetJobExecutionStatistics().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }

    public async Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData()
    {
        using var transactionScope = TransactionScopeUtility.CreateNewReadUnCommitted();
        var result = await  jobHistoryService.ListJobExecutionHistoryChartData().ConfigureAwait(false);
        transactionScope.Complete();
        return result;
    }
}