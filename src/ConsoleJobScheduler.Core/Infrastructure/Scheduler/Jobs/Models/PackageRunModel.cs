namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Models
{
    public sealed class PackageRunModel
    {
        public byte[] Content { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string Arguments { get; set; } = null!;
    }
}