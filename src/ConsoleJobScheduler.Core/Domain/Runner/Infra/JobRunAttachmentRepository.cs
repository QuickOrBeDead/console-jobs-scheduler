using System.Data;
using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz.Impl.AdoJobStore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public sealed class JobRunAttachmentRepository : IJobRunAttachmentRepository
{
    private readonly IDbAccessor _dbAccessor;
    private readonly string _dataSource;
    private readonly string _tablePrefix;

    private const string JobRunEmailInsertSql = "INSERT INTO {0}JOB_RUN_EMAIL (ID, JOB_RUN_ID, SUBJECT, BODY, MESSAGE_TO, MESSAGE_CC, MESSAGE_BCC, IS_SENT, CREATE_TIME) VALUES (@id, @jobRunId, @subject, @body, @to, @cc, @bcc, FALSE, @createTime)";

    private const string JobRunAttachmentInsertSql = "INSERT INTO {0}JOB_RUN_ATTACHMENT (JOB_RUN_ID, EMAIL_ID, NAME, CONTENT_TYPE, CONTENT, CREATE_TIME) VALUES (@jobRunId, @emailId, @name, @contentType, @content, @createTime)";

    private const string JobRunEmailIsSentUpdateSql = "UPDATE {0}JOB_RUN_EMAIL SET IS_SENT = @isSent WHERE ID = @id";

    private const string JobRunAttachmentGetSql = "SELECT CONTENT FROM {0}JOB_RUN_ATTACHMENT WHERE ID = @id";

    private const string JobRunAttachmentListSql = "SELECT ID, NAME FROM {0}JOB_RUN_ATTACHMENT WHERE JOB_RUN_ID = @jobRunId";

    public JobRunAttachmentRepository(IQuartzDbStore quartzDbStore)
    {
        _dbAccessor = quartzDbStore.GetDbAccessor();
        _dataSource = quartzDbStore.DataSource;
        _tablePrefix = quartzDbStore.TablePrefix;
    }

    public async Task SaveJobRunEmail(JobRunEmail email, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobRunEmailInsertSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
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
                await InsertJobRunAttachment(connection, attachment, attachment.EmailId, cancellationToken).ConfigureAwait(false);
            }

            connection.Commit(false);
        }
    }

    public async Task UpdateJobRunEmailIsSent(Guid id, bool isSent, CancellationToken cancellationToken = default)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobRunEmailIsSentUpdateSql, _tablePrefix);
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
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
    public async Task<byte[]?> GetJobRunAttachmentContent(long id)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(JobRunAttachmentGetSql, _tablePrefix)))
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

    public async Task InsertJobRunAttachment(JobRunAttachment attachment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadCommitted, _dataSource))
        {
            await InsertJobRunAttachment(connection, attachment, null, cancellationToken);

            connection.Commit(false);
        }
    }

    public async Task<List<JobRunAttachmentInfo>> GetJobRunAttachments(string id)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(JobRunAttachmentListSql, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "jobRunId", id);

                var result = new List<JobRunAttachmentInfo>();

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.Add(
                            new JobRunAttachmentInfo
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

    private async Task InsertJobRunAttachment(
        ConnectionAndTransactionHolder connection,
        JobRunAttachment attachment,
        Guid? emailId,
        CancellationToken cancellationToken)
    {
        var sql = AdoJobStoreUtil.ReplaceTablePrefix(JobRunAttachmentInsertSql, _tablePrefix);
        await using (var command = _dbAccessor.PrepareCommand(connection, sql))
        {
            _dbAccessor.AddCommandParameter(command, "jobRunId", attachment.JobRunId);
            _dbAccessor.AddCommandParameter(command, "name", attachment.FileName);
            _dbAccessor.AddCommandParameter(command, "content", string.IsNullOrWhiteSpace(attachment.FileContent) ? Array.Empty<byte>() : Convert.FromBase64String(attachment.FileContent));
            _dbAccessor.AddCommandParameter(command, "contentType", attachment.ContentType);
            _dbAccessor.AddCommandParameter(command, "emailId", emailId);
            _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(DateTimeOffset.UtcNow));
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}