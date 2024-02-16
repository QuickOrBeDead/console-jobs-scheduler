using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner.Exceptions;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public sealed class ConsoleAppPackageRunner : IConsoleAppPackageRunner
{
    private readonly IJobRunService _jobRunService;
    private readonly string _tempRootPath;

    public ConsoleAppPackageRunner(IJobRunService jobRunService, string tempRootPath)
    {
        if (string.IsNullOrWhiteSpace(tempRootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tempRootPath));
        }

        _jobRunService = jobRunService ?? throw new ArgumentNullException(nameof(jobRunService));
        _tempRootPath = tempRootPath;
    }

    public async Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        var packageRunModel = await _jobRunService.GetPackageRun(packageName, _tempRootPath).ConfigureAwait(false);
        if (packageRunModel == null)
        {
            throw new InvalidOperationException($"Console app package '{packageName}' not found");
        }

        try
        {
            await packageRunModel.ExtractPackage().ConfigureAwait(false);

            using (var process = new Process())
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WorkingDirectory = packageRunModel.PackageRunDirectory;
                process.StartInfo.FileName = packageRunModel.GetRunFilePath();
                process.StartInfo.Arguments = packageRunModel.GetRunArguments(arguments);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                var processOutputTasks = new List<Task>();

                process.OutputDataReceived += async (_, e) =>
                {
                    var task = ProcessOutputDataHandler(jobRunId, e.Data, false, cancellationToken);
                    processOutputTasks.Add(task);
                    await task;
                };
                process.ErrorDataReceived += async (_, e) =>
                {
                    var task = ProcessOutputDataHandler(jobRunId, e.Data, true, cancellationToken);
                    processOutputTasks.Add(task);
                    await task;
                };

                if (!process.Start())
                {
                    throw new InvalidOperationException($"Process couldn't be started: {packageRunModel.FileName}. Arguments: '{packageRunModel.GetRunArguments(arguments)}'.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                await Task.WhenAll(processOutputTasks).WaitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    throw new ConsoleAppPackageRunFailException($"Console app package '{packageName}' run failed", process.ExitCode);
                }
            }
        }
        finally
        {
            try
            {
                var directory = new DirectoryInfo(packageRunModel.PackageRunDirectory);
                if (directory.Exists)
                {
                    directory.Delete(true);
                }
            }
            catch
            {
                // Empty
            }
        }
    }

    private Task ProcessOutputDataHandler(string jobRunId, string? data, bool isError, CancellationToken cancellationToken)
    {
        return _jobRunService.ProcessJobRunConsoleMessage(jobRunId, data, isError, cancellationToken);
    }
}