using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Options;

namespace ConsoleJobScheduler.Core.Infra.Migration.Conventions;

public sealed class CustomVersionTableMetadataTable(IConventionSet conventionSet, IOptions<RunnerOptions> runnerOptions)
    : DefaultVersionTableMetaData(conventionSet, runnerOptions)
{
    public override string SchemaName => string.Empty;

    public override string TableName => "__migrations_version_info";

    public override string ColumnName => "version";

    public override string UniqueIndexName => "ux_migrations_version_info_version";

    public override string AppliedOnColumnName => "applied_on";

    public override string DescriptionColumnName => "description";

    public override bool OwnsSchema => true;
}