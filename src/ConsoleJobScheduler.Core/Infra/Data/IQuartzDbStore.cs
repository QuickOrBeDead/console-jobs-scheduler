namespace ConsoleJobScheduler.Core.Infra.Data
{
    public interface IQuartzDbStore
    {
        string DataSource { get; }

        string TablePrefix { get; }
    }
}