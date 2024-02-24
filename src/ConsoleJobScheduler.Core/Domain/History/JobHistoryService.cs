using System.Diagnostics.CodeAnalysis;
using System.Transactions;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Infra.Data;
using CronExpressionDescriptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Z.EntityFramework.Plus;

namespace ConsoleJobScheduler.Core.Domain.History;

public interface IJobHistoryService
{
    Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

    Task<JobExecutionHistoryDetail?> GetJobExecutionDetail(string id);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, Exception? jobException, CancellationToken cancellationToken = default);

    Task<JobExecutionStatistics> GetJobExecutionStatistics();

    Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData();
}

public sealed class JobHistoryService : IJobHistoryService
{
    private readonly IJobHistoryRepository _jobHistoryRepository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<JobHistoryService> _logger;

    public JobHistoryService(IJobHistoryRepository jobHistoryRepository, TimeProvider timeProvider, ILogger<JobHistoryService> logger)
    {
        _jobHistoryRepository = jobHistoryRepository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        var result = await _jobHistoryRepository.Queryable().List(pageSize, page,
            q => q.Select(
                x => new JobExecutionHistoryListItem
                {
                    Id = x.Id,
                    TriggerGroup = x.TriggerGroup,
                    TriggerName = x.TriggerName,
                    JobName = x.JobName,
                    JobGroup = x.JobGroup,
                    ScheduledTime = x.ScheduledTime,
                    FiredTime = x.FiredTime,
                    RunTime = x.RunTime,
                    Completed = x.Completed,
                    Vetoed = x.Vetoed,
                    HasError = x.HasError,
                    LastSignalTime = x.LastSignalTime,
                    NextFireTime = x.NextFireTime
                }).OrderByDescending(x => x.Id),
            q =>
            {
                return !string.IsNullOrWhiteSpace(jobName) ? q.Where(x => x.JobName == jobName) : q;
            }).ConfigureAwait(false);

        foreach (var jobExecutionHistory in result.Items)
        {
            jobExecutionHistory.UpdateHasSignalTimeout(_timeProvider.GetUtcNow());
        }

        return result;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobExecutionHistoryDetail?> GetJobExecutionDetail(string id)
    {
        var result = await _jobHistoryRepository.FindExecutionHistoryAsNoTracking<JobExecutionHistoryDetail>(id, x => new JobExecutionHistoryDetail
        {
            Id = x.Id,
            InstanceName = x.InstanceName,
            TriggerGroup = x.TriggerGroup,
            TriggerName = x.TriggerName,
            JobName = x.JobName,
            JobGroup = x.JobGroup,
            PackageName = x.PackageName,
            CronExpressionDescription = x.CronExpressionString,
            ScheduledTime = x.ScheduledTime,
            FiredTime = x.FiredTime,
            RunTime = x.RunTime,
            Completed = x.Completed,
            Vetoed = x.Vetoed,
            HasError = x.HasError,
            LastSignalTime = x.LastSignalTime,
            ErrorMessage = x.ErrorMessage
        }).ConfigureAwait(false);

        if (result != null)
        {
            result.CronExpressionDescription = GetCronExpressionDescription(result.CronExpressionDescription);
        }

        return result;
    }

    public Task<string?> GetJobExecutionErrorDetail(string id)
    {
        return _jobHistoryRepository.FindExecutionHistoryAsNoTracking(id, x => x.ErrorDetails);
    }

    public Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, Exception? jobException, CancellationToken cancellationToken = default)
    {
        if (jobException == null)
        {
            return _jobHistoryRepository.SetCompleted(id, runTime, cancellationToken);
        }

        return _jobHistoryRepository.SetCompleted(id, runTime, jobException.Message, jobException.ToString(), cancellationToken);
    }

    public async Task<JobExecutionStatistics> GetJobExecutionStatistics()
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled);

        var totalExecutedJobsCount = _jobHistoryRepository.Queryable().Where(x => !x.Vetoed).DeferredCount();
        var totalVetoedJobsCount = _jobHistoryRepository.Queryable().Where(x => x.Vetoed).DeferredCount();
        var totalSucceededJobsCount = _jobHistoryRepository.Queryable().Where(x => !x.HasError && x.Completed && !x.Vetoed).DeferredCount();
        var totalFailedJobsCount = _jobHistoryRepository.Queryable().Where(x => x.HasError).DeferredCount();
        var result = new JobExecutionStatistics
        {
            TotalExecutedJobs = await totalExecutedJobsCount.ExecuteAsync().ConfigureAwait(false),
            TotalVetoedJobs =  await totalVetoedJobsCount.ExecuteAsync().ConfigureAwait(false),
            TotalSucceededJobs = await totalSucceededJobsCount.ExecuteAsync().ConfigureAwait(false),
            TotalFailedJobs = await totalFailedJobsCount.ExecuteAsync().ConfigureAwait(false)
        };

        transactionScope.Complete();

        return result;
    }

    public Task<List<JobExecutionHistoryChartData>> ListJobExecutionHistoryChartData()
    {
        var today = _timeProvider.GetUtcNow().Date;
        var to = today.AddDays(1);

        return _jobHistoryRepository.Queryable()
            .Where(x => x.FiredTime >= today && x.FiredTime < to)
            .GroupBy(x => new
            {
                x.FiredTime.Hour,
                Minute = (x.FiredTime.Minute / 15) * 15
            })
            .Select(x =>
                new JobExecutionHistoryChartData {
                X = new DateTime(today.Year, today.Month, today.Day, x.Key.Hour, x.Key.Minute, 0, DateTimeKind.Utc), 
                Y = x.Count()
            })
            .ToListAsync();
    }

    private string GetCronExpressionDescription(string? cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
        {
            return string.Empty;
        }

        try
        {
            return ExpressionDescriptor.GetDescription(cronExpression);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Cron expression '{cronExpression}' get description error.", cronExpression);
            return string.Empty;
        }
    }
}