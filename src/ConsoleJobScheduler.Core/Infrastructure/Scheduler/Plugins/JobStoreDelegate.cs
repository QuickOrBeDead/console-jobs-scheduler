using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text.Json;

using ConsoleJobScheduler.Core.Infrastructure.Data;
using ConsoleJobScheduler.Core.Infrastructure.Extensions;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Extensions;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Models;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Models;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins.Models;

using Quartz;
using Quartz.Impl.AdoJobStore;
using Quartz.Util;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins;

public interface IJobStoreDelegate
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

    Task<PagedResult<JobExecutionHistory>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1);

    Task<JobExecutionDetail?> GetJobExecutionDetail(string id);

    Task<string?> GetJobExecutionErrorDetail(string id);

    Task InsertJobRunLog(
        string jobRunId,
        string content,
        bool isError,
        CancellationToken cancellationToken = default);

    Task<List<LogLine>> GetJobRunLogs(string id);

    Task InsertJobRunAttachment(
        AttachmentModel attachment,
        CancellationToken cancellationToken = default);

    Task<List<AttachmentInfoModel>> GetJobRunAttachments(string id);

    Task InsertJobRunEmail(
        EmailModel email,
        CancellationToken cancellationToken = default);

    Task UpdateJobRunEmailIsSent(
        Guid id,
        bool isSent,
        CancellationToken cancellationToken = default);

    Task<byte[]?> GetJobRunAttachmentContent(long id);

    Task<PackageRunModel?> GetPackageRun(string name);

    Task<List<string>> ListPackageNames();

    Task<PackageDetailsModel?> GetPackageDetails(string packageName);

    Task SavePackage(string packageName, byte[] content);

    Task<PagedResult<PackageListItemModel>> ListPackages(int pageSize = 10, int page = 1);
 
    Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData();
}

public sealed class JobStoreDelegate : IJobStoreDelegate
{
    public const string JobStoreDelegateContextKey = "quartz.JobStoreDelegate";

    private readonly IScheduler _scheduler;

    private readonly IDbAccessor _dbAccessor;

    private readonly string _dataSource;

    private readonly string _tablePrefix;
    private static readonly JsonSerializerOptions PackageManifestJsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

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
                                    SELECT ID, JOB_NAME, INSTANCE_NAME, JOB_GROUP, PACKAGE_NAME, TRIGGER_NAME, TRIGGER_GROUP, FIRED_TIME, SCHED_TIME, RUN_TIME, LAST_SIGNAL_TIME, HAS_ERROR, ERROR_MESSAGE, VETOED, COMPLETED
                                    FROM {0}JOB_HISTORY 
                                    WHERE ID = @id";

    private const string SqlJobExecutionErrorDetails = @"SELECT ERROR_DETAILS FROM {0}JOB_HISTORY WHERE ID = @id";

    private const string SqlInsertJobRunLog = "INSERT INTO {0}JOB_RUN_LOG (JOB_RUN_ID, CONTENT, IS_ERROR, CREATE_TIME) VALUES (@jobRunId, @content, @isError, @createTime)";

    private const string SqlListJobRunLog = "SELECT CONTENT, IS_ERROR FROM {0}JOB_RUN_LOG WHERE JOB_RUN_ID = @id ORDER BY ID";

    private const string SqlInsertJobRunAttachment = "INSERT INTO {0}JOB_RUN_ATTACHMENT (JOB_RUN_ID, EMAIL_ID, NAME, CONTENT_TYPE, CONTENT, CREATE_TIME) VALUES (@jobRunId, @emailId, @name, @contentType, @content, @createTime)";

    private const string SqlListJobRunAttachment = "SELECT ID, NAME FROM {0}JOB_RUN_ATTACHMENT WHERE JOB_RUN_ID = @jobRunId";

    private const string SqlGetJobRunAttachment = "SELECT CONTENT FROM {0}JOB_RUN_ATTACHMENT WHERE ID = @id";

    private const string SqlInsertJobRunEmail = "INSERT INTO {0}JOB_RUN_EMAIL (ID, JOB_RUN_ID, SUBJECT, BODY, MESSAGE_TO, MESSAGE_CC, MESSAGE_BCC, IS_SENT, CREATE_TIME) VALUES (@id, @jobRunId, @subject, @body, @to, @cc, @bcc, FALSE, @createTime)";

    private const string SqlUpdateJobRunEmailIsSent = "UPDATE {0}JOB_RUN_EMAIL SET IS_SENT = @isSent WHERE ID = @id";

    private const string SqlGetPackageContent = "SELECT CONTENT, FILE_NAME, ARGUMENTS FROM {0}PACKAGES WHERE NAME = @name";

    private const string SqlGetAllPackageNames = "SELECT NAME FROM {0}PACKAGES ORDER BY NAME";

    private const string SqlGetPackageDetail = "SELECT NAME, CREATE_TIME FROM {0}PACKAGES WHERE NAME = @name";

    private const string SqlPackageExists = "SELECT NAME FROM {0}PACKAGES WHERE NAME = @name";

    private const string SqlPackageUpdate = "UPDATE {0}PACKAGES SET CONTENT = @content, AUTHOR = @author, DESCRIPTION = @description, VERSION = @version, FILE_NAME = @fileName, ARGUMENTS = @arguments, CREATE_TIME = @createTime WHERE NAME = @name";

    private const string SqlPackageInsert = "INSERT INTO {0}PACKAGES (NAME, CONTENT, AUTHOR, DESCRIPTION, VERSION, FILE_NAME, ARGUMENTS, CREATE_TIME) VALUES (@name, @content, @author, @description, @version, @fileName, @arguments, @createTime)";

    private const string SqlListPackages = @"
                                    WITH CNT AS (SELECT COUNT(*) COUNT FROM {0}PACKAGES)
                                    SELECT NAME, (SELECT COUNT FROM CNT) COUNT
                                    FROM {0}PACKAGES
                                    ORDER BY NAME
                                    LIMIT @pageSize
                                    OFFSET @offset";

    private const string SqlJobRunHistoryChart = @"
        SELECT
           DATE_BIN('15 minutes', TO_TIMESTAMP(((FIRED_TIME - 621355968000000000) / 10000000)), TIMESTAMP '2001-01-01') AS DATE,
           COUNT(1)
        FROM {0}JOB_HISTORY
        WHERE FIRED_TIME >= @start AND FIRED_TIME < @end
        GROUP BY DATE";

    public JobStoreDelegate(IScheduler scheduler, IDbAccessor dbAccessor, string dataSource, string tablePrefix)
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
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
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
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
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

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task UpdateJobHistoryEntryLastSignalTime(string id, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobLastSignalTime, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "lastSignalTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));

                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<JobExecutionHistory>> ListJobExecutionHistory(string jobName = "", int pageSize = 10, int page = 1)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlJobExecutionHistory, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "pageSize", pageSize);
                _dbAccessor.AddCommandParameter(command, "offset", (page - 1) * pageSize);
                _dbAccessor.AddCommandParameter(command, "jobName", jobName);

                var result = new List<JobExecutionHistory>();
                var count = 0;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

                        jobExecutionHistory.UpdateHasSignalTimeout();

                        result.Add(jobExecutionHistory);
                    }
                }

                connection.Commit(false);

                return new PagedResult<JobExecutionHistory>(result, pageSize, page, count);
            }
        }
    }

    public async Task<List<(DateTime Date, int Count)>> ListJobExecutionHistoryChartData()
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
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
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
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
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
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

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
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
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "jobRunId", jobRunId);
                _dbAccessor.AddCommandParameter(command, "content", content);
                _dbAccessor.AddCommandParameter(command, "isError", _dbAccessor.GetDbBooleanValue(isError));
                _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<List<LogLine>> GetJobRunLogs(string id)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlListJobRunLog, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                var result = new List<LogLine>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(
                            new LogLine
                            {
                                Message = reader.GetString("CONTENT"),
                                IsError = reader.GetBoolean("IS_ERROR")
                            });
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task InsertJobRunAttachment(
        AttachmentModel attachment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            await InsertJobRunAttachment(connection, attachment, cancellationToken);

            connection.Commit(false);
        }
    }

    public async Task<List<AttachmentInfoModel>> GetJobRunAttachments(string id)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlListJobRunAttachment, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "jobRunId", id);

                var result = new List<AttachmentInfoModel>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(
                            new AttachmentInfoModel
                            {
                                Id = reader.GetInt64("ID"),
                                FileName = reader.GetString("NAME")
                            });
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlGetJobRunAttachment, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);

                byte[]? result = null;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = (byte[])reader.GetValue("CONTENT");
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task InsertJobRunEmail(
        EmailModel email,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlInsertJobRunEmail, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", email.Id);
                _dbAccessor.AddCommandParameter(command, "jobRunId", email.JobRunId);
                _dbAccessor.AddCommandParameter(command, "subject", email.Subject ?? string.Empty);
                _dbAccessor.AddCommandParameter(command, "body", email.Body ?? string.Empty);
                _dbAccessor.AddCommandParameter(command, "to", email.To ?? string.Empty);
                _dbAccessor.AddCommandParameter(command, "cc", email.CC ?? string.Empty);
                _dbAccessor.AddCommandParameter(command, "bcc", email.Bcc ?? string.Empty);
                _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            foreach (var attachment in email.Attachments)
            {
                attachment.EmailId = email.Id;
                await InsertJobRunAttachment(connection, attachment, cancellationToken).ConfigureAwait(false);
            }

            connection.Commit(false);
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task UpdateJobRunEmailIsSent(
        Guid id,
        bool isSent,
        CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobRunEmailIsSent, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "id", id);
                _dbAccessor.AddCommandParameter(command, "isSent", _dbAccessor.GetDbBooleanValue(isSent));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PackageRunModel?> GetPackageRun(string name)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlGetPackageContent, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "name", name);

                PackageRunModel? result;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = new PackageRunModel
                        {
                            Content = (byte[])reader.GetValue("CONTENT"),
                            FileName = reader.GetString("FILE_NAME"),
                            Arguments = reader.GetString("ARGUMENTS")
                        };
                    }
                    else
                    {
                        result = null;
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task<List<string>> ListPackageNames()
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlGetAllPackageNames, _tablePrefix)))
            {
                var result = new List<string>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(reader.GetString("NAME"));
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task<PackageDetailsModel?> GetPackageDetails(string packageName)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlGetPackageDetail, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "name", packageName);

                PackageDetailsModel? result = null;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = new PackageDetailsModel
                        {
                            Name = reader.GetString("NAME"),
                            ModifyDate = new DateTime(reader.GetInt64("CREATE_TIME"), DateTimeKind.Utc).ToLocalTime()
                        };
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }

    public async Task SavePackage(string packageName, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(packageName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
        }

        using (var connection = GetConnection(IsolationLevel.Serializable))
        {
            if (await PackageExists(connection, packageName).ConfigureAwait(false))
            {
                await UpdatePackage(connection, packageName, content).ConfigureAwait(false);
            }
            else
            {
                await InsertPackage(connection, packageName, content).ConfigureAwait(false);
            }
            connection.Commit(false);
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<PackageListItemModel>> ListPackages(int pageSize = 10, int page = 1)
    {
        using (var connection = GetConnection(IsolationLevel.ReadUncommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlListPackages, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "pageSize", pageSize);
                _dbAccessor.AddCommandParameter(command, "offset", (page - 1) * pageSize);

                var result = new List<PackageListItemModel>();
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

                        result.Add(new PackageListItemModel { Name = reader.GetString("NAME") });
                    }
                }

                connection.Commit(false);

                return new PagedResult<PackageListItemModel>(result, pageSize, page, count);
            }
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    private async Task InsertPackage(ConnectionAndTransactionHolder connection, string packageName, byte[] content)
    {
        await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlPackageInsert, _tablePrefix)))
        {
            var packageManifest = await GetPackageManifest(content).ConfigureAwait(false);

            _dbAccessor.AddCommandParameter(command, "name", packageName);
            _dbAccessor.AddCommandParameter(command, "author", packageManifest.Author);
            _dbAccessor.AddCommandParameter(command, "description", packageManifest.Description);
            _dbAccessor.AddCommandParameter(command, "version", packageManifest.Version);
            _dbAccessor.AddCommandParameter(command, "fileName", packageManifest.StartInfo.FileName);
            _dbAccessor.AddCommandParameter(command, "arguments", packageManifest.StartInfo.Arguments);
            _dbAccessor.AddCommandParameter(command, "content", content);
            _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));

            await command.ExecuteNonQueryAsync();
        }
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    private async Task UpdatePackage(ConnectionAndTransactionHolder connection, string packageName, byte[] content)
    {
        await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlPackageUpdate, _tablePrefix)))
        {
            var packageManifest = await GetPackageManifest(content).ConfigureAwait(false);

            _dbAccessor.AddCommandParameter(command, "name", packageName);
            _dbAccessor.AddCommandParameter(command, "author", packageManifest.Author);
            _dbAccessor.AddCommandParameter(command, "description", packageManifest.Description);
            _dbAccessor.AddCommandParameter(command, "version", packageManifest.Version);
            _dbAccessor.AddCommandParameter(command, "fileName", packageManifest.StartInfo.FileName);
            _dbAccessor.AddCommandParameter(command, "arguments", packageManifest.StartInfo.Arguments);
            _dbAccessor.AddCommandParameter(command, "content", content);
            _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }

    private static async Task<PackageManifest> GetPackageManifest(byte[] packageContent)
    {
        PackageManifest packageManifest;

        await using (var stream = new MemoryStream(packageContent))
        {
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
            {
                var manifestJsonEntry = zipArchive.GetEntry("manifest.json");
                if (manifestJsonEntry != null)
                {
                    using (var reader = new StreamReader(manifestJsonEntry.Open()))
                    {
                        packageManifest = JsonSerializer.Deserialize<PackageManifest>(await reader.ReadToEndAsync().ConfigureAwait(false), PackageManifestJsonSerializerOptions) ?? throw new InvalidOperationException();
                        packageManifest.Validate();
                    }
                }
                else
                {
                    throw new InvalidOperationException("manifest.json not found in package zip file.");
                }
            }
        }

        return packageManifest;
    }

    private async Task<bool> PackageExists(ConnectionAndTransactionHolder connection, string packageName)
    {
        await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(SqlPackageExists, _tablePrefix)))
        {
            _dbAccessor.AddCommandParameter(command, "name", packageName);

            await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                return await reader.ReadAsync().ConfigureAwait(false);
            }
        }
    }

    private async Task InsertJobRunAttachment(
        ConnectionAndTransactionHolder connection,
        AttachmentModel attachment,
        CancellationToken cancellationToken)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlInsertJobRunAttachment, _tablePrefix);
        await using (var command = _dbAccessor.PrepareCommand(connection, sql))
        {
            _dbAccessor.AddCommandParameter(command, "jobRunId", attachment.JobRunId);
            _dbAccessor.AddCommandParameter(command, "name", attachment.FileName);
            _dbAccessor.AddCommandParameter(command, "content", string.IsNullOrWhiteSpace(attachment.FileContent) ? Array.Empty<byte>() : Convert.FromBase64String(attachment.FileContent));
            _dbAccessor.AddCommandParameter(command, "contentType", attachment.ContentType);
            _dbAccessor.AddCommandParameter(command, "emailId", attachment.EmailId);
            _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

    public async Task UpdateJobHistoryEntryVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(SqlUpdateJobVetoed, _tablePrefix);
        using (var connection = GetConnection(IsolationLevel.ReadCommitted))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
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
    private ConnectionAndTransactionHolder GetConnection(IsolationLevel isolationLevel)
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
            throw new JobPersistenceException($"Failed to obtain DB connection from data source '{_dataSource}': {e}", e);
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