using System.Transactions;
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
}

public sealed class JobHistoryApplicationService : IJobHistoryApplicationService
{
    private readonly IJobHistoryRepository _jobHistoryRepository;
    private readonly IJobHistoryService _jobHistoryService;

    public JobHistoryApplicationService(
        IJobHistoryRepository jobHistoryRepository,
        IJobHistoryService jobHistoryService)
    {
        _jobHistoryRepository = jobHistoryRepository;
        _jobHistoryService = jobHistoryService;
    }

    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);

        var result = await _jobHistoryService.ListJobExecutionHistory(jobName, pageSize, page).ConfigureAwait(false);

        transactionScope.Complete();

        return result;
    }

    public async Task InsertJobHistoryEntry(JobExecutionHistory jobExecutionHistory, CancellationToken cancellationToken = default)
    {
        await _jobHistoryRepository.Add(jobExecutionHistory, cancellationToken).ConfigureAwait(false);
        await _jobHistoryRepository.SaveChanges(cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, Exception? jobException, CancellationToken cancellationToken = default)
    {
        return _jobHistoryService.UpdateJobHistoryEntryCompleted(id, runTime, jobException, cancellationToken);
    }

    public Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default)
    {
        return _jobHistoryService.UpdateJobHistoryEntryVetoed(id, cancellationToken);
    }

    public Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default)
    {
        return _jobHistoryService.UpdateJobHistoryEntryLastSignalTime(id, signalTime, cancellationToken);
    }

    public async Task<string?> GetJobExecutionErrorDetail(string id)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);

        var result = await _jobHistoryService.GetJobExecutionErrorDetail(id).ConfigureAwait(false);

        transactionScope.Complete();

        return result;
    }
}