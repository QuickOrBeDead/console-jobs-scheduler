namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins;

using System.Data;
using System.Data.Common;

using ConsoleJobScheduler.Service.Infrastructure.Data;
using ConsoleJobScheduler.Service.Infrastructure.Extensions;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins.Models;

using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;

public interface IJobHistoryDelegate
{
    Task InsertJobHistoryEntry(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryCompleted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default);

    Task UpdateJobHistoryEntryLastSignalTime(string id, CancellationToken cancellationToken = default);

    Task<JobExecutionStatistics> GetJobExecutionStatistics();

    Task<PagedResult<JobExecutionHistory>> GetJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

    Task<JobExecutionDetail?> GetJobExecutionDetail(string id);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task InsertJobRunLog(
        string jobRunId,
        string content,
        bool isError,
        CancellationToken cancellationToken = default);
}

public class JobHistoryDelegate : IJobHistoryDelegate
{
    private readonly IScheduler _scheduler;

    private readonly IDbAccessor _dbAccessor;

    private readonly string _dataSource;

    private readonly string _tablePrefix;

    private const string SqlInsertJobExecuted =
        "INSERT INTO {0}JOB_HISTORY (ID, SCHED_NAME, INSTANCE_NAME, TRIGGER_NAME, TRIGGER_GROUP, JOB_NAME, JOB_GROUP, PACKAGE_NAME, SCHED_TIME, FIRED_TIME, LAST_SIGNAL_TIME, VETOED, HAS_ERROR, COMPLETED) VALUES (@id, @schedulerName, @instanceName, @triggerName, @triggerGroup, @jobName, @jobGroup, @packageName, @scheduledTime, @firedTime, @lastSignalTime, FALSE, FALSE, FALSE)";

    private const string SqlUpdateJobExecuted =
        "UPDATE {0}JOB_HISTORY SET RUN_TIME = @runTime, HAS_ERROR = @hasError, ERROR_MESSAGE = @errorMessage, ERROR_DETAILS = @errorDetails, COMPLETED = TRUE WHERE ID = @id";

    private const string SqlUpdateJobVetoed = "UPDATE {0}JOB_HISTORY SET VETOED = @vetoed WHERE ID = @id";

    private const string SqlUpdateJobLastSignalTime = "UPDATE {0}JOB_HISTORY SET LAST_SIGNAL_TIME = @lastSignalTime WHERE ID = @id";

    private const string SqlJobExecutionStatistics = @"SELECT
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE VETOED = FALSE) AS TOTAL_EXECUTED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}FIRED_TRIGGERS WHERE STATE = 'EXECUTING') AS TOTAL_RUNNING_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE VETOED = TRUE) AS TOTAL_VETOED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE HAS_ERROR = FALSE AND COMPLETED = TRUE AND VETOED = FALSE) AS TOTAL_SUCCEEDED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE HAS_ERROR = TRUE) AS TOTAL_FAILED_JOBS";

    private const string SqlJobExecutionHistory = @"
                                    WITH CNT AS (SELECT COUNT(*) COUNT FROM {0}JOB_HISTORY WHERE @jobName = '' OR JOB_NAME = @jobName)
                                    SELECT ID, JOB_NAME, JOB_GROUP, TRIGGER_NAME, TRIGGER_GROUP, FIRED_TIME, SCHED_TIME, RUN_TIME, LAST_SIGNAL_TIME, HAS_ERROR, VETOED, COMPLETED, (SELECT COUNT FROM CNT) COUNT
                                    FROM {0}JOB_HISTORY
                                    WHERE @jobName = '' OR JOB_NAME = @jobName
                                    ORDER BY SCHED_TIME DESC 
                                    LIMIT @pageSize 
                                    OFFSET @offset";

    private const string SqlJobExecutionDetail = @"
                                    SELECT JOB_NAME, INSTANCE_NAME, JOB_GROUP, PACKAGE_NAME, TRIGGER_NAME, TRIGGER_GROUP, FIRED_TIME, SCHED_TIME, RUN_TIME, LAST_SIGNAL_TIME, HAS_ERROR, ERROR_MESSAGE, VETOED, COMPLETED
                                    FROM {0}JOB_HISTORY 
                                    WHERE ID = @id";

    private const string SqlJobExecutionErrorDetails = @"SELECT ERROR_DETAILS FROM {0}JOB_HISTORY WHERE ID = @id";

    private const string SqlInsertJobRunLog = "INSERT INTO {0}JOB_RUN_LOG (JOB_RUN_ID, CONTENT, IS_ERROR, CREATE_TIME) VALUES (@jobRunId, @content, @isError, @createTime)";

    public JobHistoryDelegate(IScheduler scheduler, IDbAccessor dbAccessor, string dataSource, string tablePrefix)
    {
        _scheduler = scheduler;
        _dbAccessor = dbAccessor;
        _dataSource = dataSource;
        _tablePrefix = tablePrefix;
    }

    public async Task InsertJobHistoryEntry(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlInsertJobExecuted, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", context.FireInstanceId);
                _dbAccessor.AddCommandParameter(command, "schedulerName", context.Scheduler.SchedulerName);
                _dbAccessor.AddCommandParameter(command, "instanceName", context.Scheduler.SchedulerInstanceId);
                _dbAccessor.AddCommandParameter(command, "jobName", context.JobDetail.Key.Name);
                _dbAccessor.AddCommandParameter(command, "jobGroup", context.JobDetail.Key.Group);
                _dbAccessor.AddCommandParameter(command, "triggerName", context.Trigger.Key.Name);
                _dbAccessor.AddCommandParameter(command, "triggerGroup", context.Trigger.Key.Group);
                _dbAccessor.AddCommandParameter(command, "scheduledTime", _dbAccessor.GetDbDateTimeValue(context.ScheduledFireTimeUtc));
                _dbAccessor.AddCommandParameter(command, "firedTime", _dbAccessor.GetDbDateTimeValue(context.FireTimeUtc));
                _dbAccessor.AddCommandParameter(command, "lastSignalTime", _dbAccessor.GetDbDateTimeValue(context.ScheduledFireTimeUtc));
                _dbAccessor.AddCommandParameter(command, "packageName", GetJobData(context.JobDetail, "package"));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task UpdateJobHistoryEntryCompleted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobExecuted, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", context.FireInstanceId);
                _dbAccessor.AddCommandParameter(command, "runTime", _dbAccessor.GetDbTimeSpanValue(context.JobRunTime));
                _dbAccessor.AddCommandParameter(command, "hasError", _dbAccessor.GetDbBooleanValue(jobException != null));
                _dbAccessor.AddCommandParameter(command, "errorMessage", jobException?.Message);
                _dbAccessor.AddCommandParameter(command, "errorDetails", jobException?.ToString());

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task UpdateJobHistoryEntryLastSignalTime(string id, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobLastSignalTime, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "lastSignalTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task<PagedResult<JobExecutionHistory>> GetJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionHistory, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "pageSize", pageSize);
                _dbAccessor.AddCommandParameter(command, "offset", (page - 1) * pageSize);
                _dbAccessor.AddCommandParameter(command, "jobName", jobName);

                var result = new List<JobExecutionHistory>();
                var count = 0;

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var counter = 0;
                    var triggers = new Dictionary<TriggerKey, ITrigger?>();
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        if (counter == 0)
                        {
                            count = reader.GetInt32("COUNT");
                        }

                        counter++;

                        var scheduledTime = new DateTime(reader.GetInt64("SCHED_TIME"), DateTimeKind.Utc);
                        var jobExecutionHistory = new JobExecutionHistory
                                                      {
                                                          Id = reader.GetString("ID"),
                                                          JobName = reader.GetString("JOB_NAME"),
                                                          JobGroup = reader.GetString("JOB_GROUP"),
                                                          TriggerName = reader.GetString("TRIGGER_NAME"),
                                                          TriggerGroup = reader.GetString("TRIGGER_GROUP"),
                                                          ScheduledTime = scheduledTime.ToLocalTime(),
                                                          FiredTime = new DateTime(reader.GetInt64("FIRED_TIME"), DateTimeKind.Utc).ToLocalTime(),
                                                          RunTime = GetNullableTimeSpanFromNullableInt64Column(reader, "RUN_TIME"),
                                                          LastSignalTime = new DateTime(reader.GetInt64("LAST_SIGNAL_TIME"), DateTimeKind.Utc).ToLocalTime(),
                                                          Completed = reader.GetBoolean("COMPLETED"),
                                                          Vetoed = reader.GetBoolean("VETOED"),
                                                          HasError = reader.GetBoolean("HAS_ERROR")
                                                      };

                        var triggerKey = new TriggerKey(jobExecutionHistory.TriggerName, jobExecutionHistory.TriggerGroup);
                        if (!triggers.TryGetValue(triggerKey, out var trigger))
                        {
                            triggers[triggerKey] = trigger = await _scheduler.GetTrigger(triggerKey);
                        }

                        if (trigger != null)
                        {
                            jobExecutionHistory.NextFireTime = trigger.GetFireTimeAfter(scheduledTime)?.DateTime.ToLocalTime();
                        }

                        jobExecutionHistory.UpdateHasSignalTimeout(TimeSpan.FromMinutes(1));

                        result.Add(jobExecutionHistory);
                    }
                }

                connection.Commit(false);

                return new PagedResult<JobExecutionHistory>(result, pageSize, page, count);
            }
        }
    }

    public async Task<JobExecutionDetail?> GetJobExecutionDetail(string id)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionDetail, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                JobExecutionDetail? result = null;

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = new JobExecutionDetail
                                     {
                                         JobName = reader.GetString("JOB_NAME"),
                                         InstanceName = reader.GetString("INSTANCE_NAME"),
                                         JobGroup = reader.GetString("JOB_GROUP"),
                                         PackageName = reader.GetString("PACKAGE_NAME"),
                                         TriggerName = reader.GetString("TRIGGER_NAME"),
                                         TriggerGroup = reader.GetString("TRIGGER_GROUP"),
                                         ScheduledTime = new DateTime(reader.GetInt64("SCHED_TIME"), DateTimeKind.Utc).ToLocalTime(),
                                         FiredTime = new DateTime(reader.GetInt64("FIRED_TIME"), DateTimeKind.Utc).ToLocalTime(),
                                         RunTime = GetNullableTimeSpanFromNullableInt64Column(reader, "RUN_TIME"),
                                         Completed = reader.GetBoolean("COMPLETED"),
                                         Vetoed = reader.GetBoolean("VETOED"),
                                         HasError = reader.GetBoolean("HAS_ERROR"),
                                         ErrorMessage = reader.GetNullableString("ERROR_MESSAGE")
                                     };
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task<string?> GetJobExecutionErrorDetail(string id)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionErrorDetails, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        return reader.GetNullableString("ERROR_DETAILS");
                    }
                }

                connection.Commit(false);

                return null;
            }
        }
    }

    public async Task InsertJobRunLog(
        string jobRunId,
        string content,
        bool isError,
        CancellationToken cancellationToken = default)
    {
        if (isError)
        {
            content ??= string.Empty;
            content = string.Join('\n', content.Split('\n').Select(x => $"##[error] {x}"));
        }

        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlInsertJobRunLog, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "jobRunId", jobRunId);
                _dbAccessor.AddCommandParameter(command, "content", content);
                _dbAccessor.AddCommandParameter(command, "isError", isError);
                _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    private static TimeSpan? GetNullableTimeSpanFromNullableInt64Column(IDataReader reader, string columnName)
    {
        var runtime = reader.GetNullableInt64(columnName);
        return runtime.HasValue ? TimeSpan.FromMilliseconds(runtime.Value) : null;
    }

    public async Task<JobExecutionStatistics> GetJobExecutionStatistics()
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionStatistics, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                var result = new JobExecutionStatistics();
               
                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.TotalExecutedJobs = reader.GetInt32("TOTAL_EXECUTED_JOBS");
                        result.TotalRunningJobs = reader.GetInt32("TOTAL_RUNNING_JOBS");
                        result.TotalSucceededJobs = reader.GetInt32("TOTAL_SUCCEEDED_JOBS");
                        result.TotalFailedJobs = reader.GetInt32("TOTAL_FAILED_JOBS");
                        result.TotalVetoedJobs = reader.GetInt32("TOTAL_VETOED_JOBS");
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task UpdateJobHistoryEntryVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobVetoed, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", context.FireInstanceId);
                _dbAccessor.AddCommandParameter(command, "vetoed", _dbAccessor.GetDbBooleanValue(true));

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    /// <summary>
    /// Gets the connection and starts a new transaction.
    /// </summary>
    /// <param name="isolationLevel"></param>
    /// <returns></returns>
    protected virtual ConnectionAndTransactionHolder GetConnection(IsolationLevel isolationLevel)
    {
        DbConnection conn;
        DbTransaction tx;
        try
        {
            conn = DBConnectionManager.Instance.GetConnection(_dataSource);
            conn.Open();
        }
        catch (Exception e)
        {
            throw new JobPersistenceException(
                $"Failed to obtain DB connection from data source '{_dataSource}': {e}", e);
        }
        if (conn == null)
        {
            throw new JobPersistenceException($"Could not get connection from DataSource '{_dataSource}'");
        }

        try
        {
            tx = conn.BeginTransaction(isolationLevel);
        }
        catch (Exception e)
        {
            conn.Close();
            throw new JobPersistenceException("Failure setting up connection.", e);
        }

        return new ConnectionAndTransactionHolder(conn, tx);
    }

    private static string? GetJobData(IJobDetail jobDetail, string key)
    {
        if (jobDetail.JobDataMap.TryGetValue(key, out var value))
        {
            return value as string;
        }

        return null;
    }
}