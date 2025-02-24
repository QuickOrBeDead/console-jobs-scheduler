namespace ConsoleJobScheduler.Core.Infra.Migration
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