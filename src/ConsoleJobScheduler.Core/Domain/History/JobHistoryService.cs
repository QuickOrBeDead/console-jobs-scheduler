using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Infra.Data;

namespace ConsoleJobScheduler.Core.Domain.History;

public sealed class JobHistoryService : IJobHistoryService
{
    private readonly IJobHistoryRepository _jobHistoryRepository;

    public JobHistoryService(IJobHistoryRepository jobHistoryRepository)
    {
        _jobHistoryRepository = jobHistoryRepository;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        var result = await _jobHistoryRepository.ListJobExecutionHistory(jobName, pageSize, page);

        foreach (var jobExecutionHistory in result.Items)
        {
            jobExecutionHistory.UpdateHasSignalTimeout();
        }

        return result;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobExecutionDetail?> GetJobExecutionDetail(string id)
    {
        var result = await _jobHistoryRepository.GetJobExecutionDetail(id);
        if (result == null)
        {
            return null;
        }

        result.UpdateHasSignalTimeout();

        return result;
    }
}