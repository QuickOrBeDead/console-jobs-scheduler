namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs
{
    public sealed class PackageManifest
    {
        public sealed class ProgramStartInfo
        {
            public string FileName { get; set; } = null!;

            public string Arguments { get; set; } = null!;
        }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Author { get; set; } = null!;

        public string Version { get; set; } = null!;

        public ProgramStartInfo StartInfo { get; set; } = null!;

        public void Validate()
        {
            if (StartInfo == null)
            {
                throw new InvalidOperationException("startInfo cannot be null");
            }

            if (StartInfo.FileName == null)
            {
                throw new InvalidOperationException("startInfo.fileName cannot be null");
            }

            if (StartInfo.FileName == null)
            {
                throw new InvalidOperationException("startInfo.arguments cannot be null");
            }

            if (Name == null)
            {
                throw new InvalidOperationException("name cannot be null");
            }

            if (Description == null)
            {
                throw new InvalidOperationException("description cannot be null");
            }

            if (Author == null)
            {
                throw new InvalidOperationException("Author cannot be null");
            }

            if (Version == null)
            {
                throw new InvalidOperationException("version cannot be null");
            }
        }
    }
}