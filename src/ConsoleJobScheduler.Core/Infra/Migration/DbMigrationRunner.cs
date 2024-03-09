using ConsoleJobScheduler.Core.Infra.Migration.Conventions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Infra.Migration;

public interface IDbMigrationRunner
{
    void Migrate(string connectionString, string tablePrefix, string migrationGroup, string dbType = "PostgreSQL", long? version = null);
}

public sealed class DbMigrationRunner : IDbMigrationRunner
{
    public void Migrate(string connectionString, string tablePrefix, string migrationGroup, string dbType = "PostgreSQL", long? version = null)
    {
        using var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres11_0()
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DbMigrationRunner).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .AddScoped<IVersionTableMetaData, CustomVersionTableMetadataTable>()
            .AddSingleton<IMigrationContext>(_ => new MigrationContext(tablePrefix))
            .AddSingleton<IConventionSet>(new CustomConventionSet())
            .Configure<ProcessorOptions>(opt =>
            {
                opt.PreviewOnly = false;
                opt.Timeout = TimeSpan.FromSeconds(180);
            })
            .Configure<SelectingProcessorAccessorOptions>(cfg =>
            {
                cfg.ProcessorId = dbType;
            })
            .Configure<SelectingGeneratorAccessorOptions>(cfg =>
            {
                cfg.GeneratorId = dbType;
            })
            .Configure<RunnerOptions>(opt =>
            {
                opt.Tags = [migrationGroup];
            })
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

        if (version.HasValue)
        {
            runner.MigrateUp(version.Value);
        }
        else
        {
            runner.MigrateUp();
        }
    }
}