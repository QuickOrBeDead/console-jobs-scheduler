using System.Data;
using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Infra.Data;
using ConsoleJobScheduler.Core.Infra.Data.Extensions;
using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;

namespace ConsoleJobScheduler.Core.Domain.History.Infra;

public sealed class JobHistoryRepository : IJobHistoryRepository
{
     private const string JobHistoryInsertSql =
        "INSERT INTO {0}JOB_HISTORY (ID, SCHED_NAME, INSTANCE_NAME, TRIGGER_NAME, TRIGGER_GROUP, JOB_NAME, JOB_GROUP, PACKAGE_NAME, SCHED_TIME, FIRED_TIME, LAST_SIGNAL_TIME, VETOED, HAS_ERROR, COMPLETED) VALUES (@id, @schedulerName, @instanceName, @triggerName, @triggerGroup, @jobName, @jobGroup, @packageName, @scheduledTime, @firedTime, @lastSignalTime, @vetoed, @hasError, @completed)";

    private const string JobHistoryExecutedUpdateSql =
        "UPDATE {0}JOB_HISTORY SET RUN_TIME = @runTime, HAS_ERROR = @hasError, ERROR_MESSAGE = @errorMessage, ERROR_DETAILS = @errorDetails, COMPLETED = TRUE WHERE ID = @id";

    private const string JobHistoryLastSignalTimeUpdateSql = "UPDATE {0}JOB_HISTORY SET LAST_SIGNAL_TIME = @lastSignalTime WHERE ID = @id";

    private const string JobHistoryVetoedUpdateSql = "UPDATE {0}JOB_HISTORY SET VETOED = @vetoed WHERE ID = @id";

    private const string JobExecutionStatisticsSql = @"SELECT
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE VETOED = FALSE) AS TOTAL_EXECUTED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}FIRED_TRIGGERS WHERE STATE = 'EXECUTING') AS TOTAL_RUNNING_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE VETOED = TRUE) AS TOTAL_VETOED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE HAS_ERROR = FALSE AND COMPLETED = TRUE AND VETOED = FALSE) AS TOTAL_SUCCEEDED_JOBS,
                                                        (SELECT COUNT(1) FROM {0}JOB_HISTORY WHERE HAS_ERROR = TRUE) AS TOTAL_FAILED_JOBS";

    private const string JobHistoryListSql = @"
                                        WITH CNT AS (SELECT COUNT(*) COUNT FROM {0}JOB_HISTORY WHERE @jobName = '' OR JOB_NAME = @jobName)
                                        SELECT ID, JOB_NAME, JOB_GROUP, TRIGGER_NAME, TRIGGER_GROUP, FIRED_TIME, SCHED_TIME, RUN_TIME, LAST_SIGNAL_TIME, HAS_ERROR, VETOED, COMPLETED, (SELECT COUNT FROM CNT) COUNT
                                        FROM {0}JOB_HISTORY
                                        WHERE @jobName = '' OR JOB_NAME = @jobName
                                        ORDER BY SCHED_TIME DESC 
                                        LIMIT @pageSize 
                                        OFFSET @offset";

    private const string SqlJobExecutionDetail = @"
                                        SELECT ID, JOB_NAME, INSTANCE_NAME, JOB_GROUP, PACKAGE_NAME, TRIGGER_NAME, TRIGGER_GROUP, FIRED_TIME, SCHED_TIME, RUN_TIME, LAST_SIGNAL_TIME, HAS_ERROR, ERROR_MESSAGE, VETOED, COMPLETED
                                        FROM {0}JOB_HISTORY 
                                        WHERE ID = @id";

    private const string SqlJobRunHistoryChart = @"
        SELECT
           DATE_BIN('15 minutes', TO_TIMESTAMP(((FIRED_TIME - 621355968000000000) / 10000000)), TIMESTAMP '2001-01-01') AS DATE,
           COUNT(1)
        FROM {0}JOB_HISTORY
        WHERE FIRED_TIME >= @start AND FIRED_TIME < @end
        GROUP BY DATE";

    private const string SqlJobExecutionErrorDetails = @"SELECT ERROR_DETAILS FROM {0}JOB_HISTORY WHERE ID = @id";

    private readonly IDbAccessor _dbAccessor;
    private readonly string _dataSource;
    private readonly string _tablePrefix;

    public JobHistoryRepository(IQuartzDbStore quartzDbStore)
    {
        _dbAccessor = quartzDbStore.GetDbAccessor();
        _dataSource = quartzDbStore.DataSource;
        _tablePrefix = quartzDbStore.TablePrefix;
    }

    public async Task InsertJobHistoryEntry(JobExecutionHistory history, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobHistoryInsertSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", history.Id);
                _dbAccessor.AddCommandParameter(command, "schedulerName",history.SchedulerName);
                _dbAccessor.AddCommandParameter(command, "instanceName", history.InstanceName);
                _dbAccessor.AddCommandParameter(command, "jobName", history.JobName);
                _dbAccessor.AddCommandParameter(command, "jobGroup", history.JobGroup);
                _dbAccessor.AddCommandParameter(command, "packageName", history.PackageName);
                _dbAccessor.AddCommandParameter(command, "triggerName", history.TriggerName);
                _dbAccessor.AddCommandParameter(command, "triggerGroup", history.TriggerGroup);
                _dbAccessor.AddCommandParameter(command, "scheduledTime", _dbAccessor.GetDbDateTimeValue(history.ScheduledTime));
                _dbAccessor.AddCommandParameter(command, "firedTime", _dbAccessor.GetDbDateTimeValue(history.FiredTime));
                _dbAccessor.AddCommandParameter(command, "lastSignalTime", _dbAccessor.GetDbDateTimeValue(history.LastSignalTime));
                _dbAccessor.AddCommandParameter(command, "vetoed", history.Vetoed);
                _dbAccessor.AddCommandParameter(command, "hasError", history.HasError);
                _dbAccessor.AddCommandParameter(command, "completed", history.Completed);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task UpdateJobHistoryEntryCompleted(string id, TimeSpan runTime, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobHistoryExecutedUpdateSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "runTime", _dbAccessor.GetDbTimeSpanValue(runTime));
                _dbAccessor.AddCommandParameter(command, "hasError", _dbAccessor.GetDbBooleanValue(jobException != null));
                _dbAccessor.AddCommandParameter(command, "errorMessage", jobException?.Message);
                _dbAccessor.AddCommandParameter(command, "errorDetails", jobException?.ToString());

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task UpdateJobHistoryEntryVetoed(string id, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobHistoryVetoedUpdateSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "vetoed", _dbAccessor.GetDbBooleanValue(true));

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task UpdateJobHistoryEntryLastSignalTime(string id, DateTime signalTime, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobHistoryLastSignalTimeUpdateSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "lastSignalTime", _dbAccessor.GetDbDateTimeValue(signalTime));

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<JobExecutionHistoryListItem>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(JobHistoryListSql, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "pageSize", pageSize);
                _dbAccessor.AddCommandParameter(command, "offset", (page - 1) * pageSize);
                _dbAccessor.AddCommandParameter(command, "jobName", jobName);

                var result = new List<JobExecutionHistoryListItem>();
                var count = 0;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var counter = 0;

                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        if (counter == 0)
                        {
                            count = reader.GetInt32("COUNT");
                        }

                        counter++;

                        var jobExecutionHistory = new JobExecutionHistoryListItem
                        {
                            Id = reader.GetString("ID"),
                            JobName = reader.GetString("JOB_NAME"),
                            JobGroup = reader.GetString("JOB_GROUP"),
                            TriggerName = reader.GetString("TRIGGER_NAME"),
                            TriggerGroup = reader.GetString("TRIGGER_GROUP"),
                            ScheduledTime = new DateTime(reader.GetInt64("SCHED_TIME"), DateTimeKind.Utc).ToLocalTime(),
                            FiredTime = new DateTime(reader.GetInt64("FIRED_TIME"), DateTimeKind.Utc).ToLocalTime(),
                            RunTime = GetNullableTimeSpanFromNullableInt64Column(reader, "RUN_TIME"),
                            LastSignalTime = new DateTime(reader.GetInt64("LAST_SIGNAL_TIME"), DateTimeKind.Utc).ToLocalTime(),
                            Completed = reader.GetBoolean("COMPLETED"),
                            Vetoed = reader.GetBoolean("VETOED"),
                            HasError = reader.GetBoolean("HAS_ERROR")
                        };

                        result.Add(jobExecutionHistory);
                    }
                }

                connection.Commit(false);

                return new PagedResult<JobExecutionHistoryListItem>(result, pageSize, page, count);
            }
        }
    }

    public async Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData()
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobRunHistoryChart, _tablePrefix)))
            {
                var today = DateTime.UtcNow.Date;
                _dbAccessor.AddCommandParameter(command, "start", today.Ticks);
                _dbAccessor.AddCommandParameter(command, "end", today.AddDays(1).Ticks);

                var result = new List<(DateTime, int)>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add((reader.GetDateTime("DATE").ToLocalTime(), reader.GetInt32("COUNT")));
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobExecutionDetail?> GetJobExecutionDetail(string id)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionDetail, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                JobExecutionDetail? result = null;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = new JobExecutionDetail
                        {
                            Id = reader.GetString("ID"),
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
                            ErrorMessage = reader.GetNullableString("ERROR_MESSAGE"),
                            LastSignalTime = new DateTime(reader.GetInt64("LAST_SIGNAL_TIME"), DateTimeKind.Utc).ToLocalTime()
                        };

                        result.UpdateHasSignalTimeout();
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<string?> GetJobExecutionErrorDetail(string id)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionErrorDetails, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

    public async Task<JobExecutionStatistics> GetJobExecutionStatistics()
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobExecutionStatisticsSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                var result = new JobExecutionStatistics();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

    private static TimeSpan? GetNullableTimeSpanFromNullableInt64Column(IDataReader reader, string columnName)
    {
        var runtime = reader.GetNullableInt64(columnName);
        return runtime.HasValue ? TimeSpan.FromMilliseconds(runtime.Value) : null;
    }
}