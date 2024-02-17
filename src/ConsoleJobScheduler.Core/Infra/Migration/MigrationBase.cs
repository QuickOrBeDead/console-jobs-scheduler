namespace ConsoleJobScheduler.Core.Infra.Migration;

public abstract class MigrationBase(IMigrationContext migrationContext) : FluentMigrator.Migration
{
    private readonly IMigrationContext _migrationContext = migrationContext ?? throw new ArgumentNullException(nameof(migrationContext));

    protected string GetNameWithTablePrefix(string name)
    {
        return $"{_migrationContext.TablePrefix}{name}";
    }

    protected string GetIndexNameWithTablePrefix(string name)
    {
        return $"idx_{_migrationContext.TablePrefix}{name}";
    }
}