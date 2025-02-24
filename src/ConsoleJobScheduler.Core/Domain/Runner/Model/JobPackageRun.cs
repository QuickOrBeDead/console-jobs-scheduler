using System.IO.Compression;

namespace ConsoleJobScheduler.Core.Domain.Runner.Model
{
    public sealed class JobPackageRun
    {
        private readonly string _tempDirectory;

        private byte[] Content { get; set; }

        public string FileName { get; set; }

        public string Arguments { get; set; }

        private string PackageName { get; set; }

        public string PackageRunDirectory { get; set; }

        public JobPackageRun(string name, byte[] content, string fileName, string arguments, string tempRootPath)
        {
            if (string.IsNullOrWhiteSpace(tempRootPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(tempRootPath));
            }

            PackageName = name;
            Content = content;
            FileName = fileName;
            Arguments = arguments;

            _tempDirectory = Path.Combine(tempRootPath, "Temp");

            PackageRunDirectory = Path.Combine(_tempDirectory, PackageName, Guid.NewGuid().ToString("N"));
        }

        public string GetRunFilePath()
        {
            return FileName.StartsWith("./", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(PackageRunDirectory, FileName)
                : FileName;
        }

        public string GetRunArguments(string arguments)
        {
            return string.IsNullOrWhiteSpace(Arguments)
                ? arguments
                : $"{Arguments} {arguments}";
        }

        public async Task ExtractPackage()
        {
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }

            await using (var stream = new MemoryStream(Content))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                {
                    zipArchive.ExtractToDirectory(PackageRunDirectory);
                }
            }
        }

        public void DeletePackageRun()
        {
            var directory = new DirectoryInfo(PackageRunDirectory);
            if (directory.Exists)
            {
                directory.Delete(true);
            }
        }
    }
}