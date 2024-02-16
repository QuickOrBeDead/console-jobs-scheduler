using FluentMigrator.Runner.VersionTableInfo;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Migrations.Core.Conventions;

public sealed class CustomVersionTableMetadataTable : DefaultVersionTableMetaData
{
    public override string SchemaName => string.Empty;

    public override string TableName => "__migrations_version_info";

    public override string ColumnName => "version";

    public override string UniqueIndexName => "ux_migrations_version_info_version";

    public override string AppliedOnColumnName => "applied_on";

    public override string DescriptionColumnName => "description";

    public override bool OwnsSchema => true;
}