namespace ConsoleJobScheduler.Core.Domain.Scheduler.Migrations.Core
{
    public interface IMigrationContext
    {
        string TablePrefix { get; }
    }

    public sealed class MigrationContext(string tablePrefix) : IMigrationContext
    {
        public string TablePrefix { get; } = tablePrefix;
    }
}