using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Infra.Data;

namespace ConsoleJobScheduler.Core.Domain.History
{
    public interface IJobHistoryService
    {
        Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

        Task<JobExecutionDetail?> GetJobExecutionDetail(string id);
    }
}