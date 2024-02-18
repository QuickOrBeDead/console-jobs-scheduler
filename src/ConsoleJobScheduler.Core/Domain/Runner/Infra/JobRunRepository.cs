using System.Data;
using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz.Impl.AdoJobStore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public sealed class JobRunRepository : IJobRunRepository
{
    private readonly IDbAccessor _dbAccessor;
    private readonly string _dataSource;
    private readonly string _tablePrefix;

    private const string JobRunLogInsertSql = "INSERT INTO {0}JOB_RUN_LOG (JOB_RUN_ID, CONTENT, IS_ERROR, CREATE_TIME) VALUES (@jobRunId, @content, @isError, @createTime)";

    private const string JobRunLogListSql = "SELECT CONTENT, IS_ERROR, CREATE_TIME FROM {0}JOB_RUN_LOG WHERE JOB_RUN_ID = @id ORDER BY ID";

    public JobRunRepository(IQuartzDbStore quartzDbStore)
    {
        _dbAccessor = quartzDbStore.GetDbAccessor();
        _dataSource = quartzDbStore.DataSource;
        _tablePrefix = quartzDbStore.TablePrefix;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task SaveJobRunLog(JobRunLog jobRunLog, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobRunLogInsertSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, sql))
            {
                _dbAccessor.AddCommandParameter(command, "jobRunId", jobRunLog.JobRunId);
                _dbAccessor.AddCommandParameter(command, "content", jobRunLog.Content);
                _dbAccessor.AddCommandParameter(command, "isError", _dbAccessor.GetDbBooleanValue(jobRunLog.IsError));
                _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(jobRunLog.CreateDate));
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                connection.Commit(false);
            }
        }
    }

    public async Task<List<JobRunLog>> GetJobRunLogs(string jobRunId)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(JobRunLogListSql, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "id", jobRunId);

                var result = new List<JobRunLog>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(new JobRunLog(jobRunId,
                            reader.GetString("CONTENT"),
                            reader.GetBoolean("IS_ERROR"),
                            new DateTime(reader.GetInt64("CREATE_TIME"), DateTimeKind.Utc).ToLocalTime()));
                    }
                }

                connection.Commit(false);

                return result;
            }
        }
    }
}