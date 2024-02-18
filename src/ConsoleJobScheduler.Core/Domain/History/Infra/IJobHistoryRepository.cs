using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz;

namespace ConsoleJobScheduler.Core.Domain.History.Infra
{
    public interface IJobHistoryRepository
    {
        Task InsertJobHistoryEntry(JobExecutionHistory history, CancellationToken cancellationToken = default);

        Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, JobExecutionException? jobException, CancellationToken cancellationToken = default);

        Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default);

        Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default);

        Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

        Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData();

        Task<JobExecutionDetail?> GetJobExecutionDetail(string id);

        Task<string?> GetJobExecutionErrorDetail(string id);

        Task<JobExecutionStatistics> GetJobExecutionStatistics();
    }
}