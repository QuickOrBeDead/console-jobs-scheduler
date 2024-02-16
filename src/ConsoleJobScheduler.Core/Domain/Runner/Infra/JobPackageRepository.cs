using System.Data;
using System.Diagnostics.CodeAnalysis;
using ConsoleJobScheduler.Core.Domain.Runner.Model;
using ConsoleJobScheduler.Core.Infra.Data;
using Quartz.Impl.AdoJobStore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public sealed class JobPackageRepository : IJobPackageRepository
{
    private readonly IDbAccessor _dbAccessor;
    private readonly string _dataSource;
    private readonly string _tablePrefix;

    private const string PackageGetByNameSql = "SELECT NAME, DESCRIPTION, AUTHOR, VERSION, CONTENT, FILE_NAME, ARGUMENTS, CREATE_TIME FROM {0}PACKAGES WHERE NAME = @name";

    private const string PackageNamesGetAllSql = "SELECT NAME FROM {0}PACKAGES ORDER BY NAME";

    private const string PackageDetailGetSql = "SELECT NAME, CREATE_TIME FROM {0}PACKAGES WHERE NAME = @name";

    private const string PackageExistsSql = "SELECT NAME FROM {0}PACKAGES WHERE NAME = @name";

    private const string PackageUpdateSql = "UPDATE {0}PACKAGES SET CONTENT = @content, AUTHOR = @author, DESCRIPTION = @description, VERSION = @version, FILE_NAME = @fileName, ARGUMENTS = @arguments, CREATE_TIME = @createTime WHERE NAME = @name";

    private const string PackageInsertSql = "INSERT INTO {0}PACKAGES (NAME, CONTENT, AUTHOR, DESCRIPTION, VERSION, FILE_NAME, ARGUMENTS, CREATE_TIME) VALUES (@name, @content, @author, @description, @version, @fileName, @arguments, @createTime)";

    private const string PackagesListSql = @"
                                        WITH CNT AS (SELECT COUNT(*) COUNT FROM {0}PACKAGES)
                                        SELECT NAME, (SELECT COUNT FROM CNT) COUNT
                                        FROM {0}PACKAGES
                                        ORDER BY NAME
                                        LIMIT @pageSize
                                        OFFSET @offset";

    public JobPackageRepository(IQuartzDbStore quartzDbStore)
    {
        _dbAccessor = quartzDbStore.GetDbAccessor();
        _dataSource = quartzDbStore.DataSource;
        _tablePrefix = quartzDbStore.TablePrefix;
    }

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<JobPackage?> GetByName(string name)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(PackageGetByNameSql, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "name", name);

                JobPackage? result;

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result = new JobPackage
                        {
                            Name = name,
                            Description = reader.GetString("DESCRIPTION"),
                            Content = (byte[])reader.GetValue("CONTENT"),
                            FileName = reader.GetString("FILE_NAME"),
                            Author = reader.GetString("AUTHOR"),
                            Version = reader.GetString("VERSION"),
                            Arguments = reader.GetString("ARGUMENTS"),
                            CreateDate = new DateTime(reader.GetInt64("CREATE_TIME"), DateTimeKind.Utc).ToLocalTime()
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

    public async Task SavePackage(JobPackage jobPackage)
    {
        ArgumentNullException.ThrowIfNull(jobPackage);

        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.Serializable, _dataSource))
        {
            if (await PackageExists(connection, jobPackage.Name).ConfigureAwait(false))
            {
                await SavePackage(connection, PackageUpdateSql, jobPackage).ConfigureAwait(false);
            }
            else
            {
                await SavePackage(connection, PackageInsertSql, jobPackage).ConfigureAwait(false);
            }
            connection.Commit(false);
        }
    }

    public async Task<List<string>> GetAllPackageNames()
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(PackageNamesGetAllSql, _tablePrefix)))
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

    public async Task<PackageDetailsModel?> GetPackageDetails(string name)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(PackageDetailGetSql, _tablePrefix)))
            {
                _dbAccessor.AddCommandParameter(command, "name", name);

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

    [SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "<Pending>")]
    public async Task<PagedResult<PackageListItemModel>> ListPackages(int pageSize = 10, int page = 1)
    {
        using (var connection = ConnectionUtility.GetConnection(IsolationLevel.ReadUncommitted, _dataSource))
        {
            await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(PackagesListSql, _tablePrefix)))
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
    private async Task SavePackage(ConnectionAndTransactionHolder connection, string query, JobPackage package)
    {
        await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(query, _tablePrefix)))
        {
            _dbAccessor.AddCommandParameter(command, "name", package.Name);
            _dbAccessor.AddCommandParameter(command, "author", package.Author);
            _dbAccessor.AddCommandParameter(command, "description", package.Description);
            _dbAccessor.AddCommandParameter(command, "version", package.Version);
            _dbAccessor.AddCommandParameter(command, "fileName", package.FileName);
            _dbAccessor.AddCommandParameter(command, "arguments", package.Arguments);
            _dbAccessor.AddCommandParameter(command, "content", package.Content);
            _dbAccessor.AddCommandParameter(command, "createTime", _dbAccessor.GetDbDateTimeValue(package.CreateDate));

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<bool> PackageExists(ConnectionAndTransactionHolder connection, string packageName)
    {
        await using (var command = _dbAccessor.PrepareCommand(connection, AdoJobStoreUtil.ReplaceTablePrefix(PackageExistsSql, _tablePrefix)))
        {
            _dbAccessor.AddCommandParameter(command, "name", packageName);

            await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
            {
                return await reader.ReadAsync().ConfigureAwait(false);
            }
        }
    }
}