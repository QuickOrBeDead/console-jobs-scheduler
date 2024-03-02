using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner.Exceptions;
using ConsoleJobScheduler.Messaging;
using Microsoft.Extensions.Configuration;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IConsoleAppPackageRunner
{
    Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}

public sealed class ConsoleAppPackageRunner(
    IJobRunService jobRunService,
    IConsoleMessageProcessorManager consoleMessageProcessorManager,
    IProcessRunnerFactory processRunnerFactory,
    IConfiguration configuration)
    : IConsoleAppPackageRunner
{
    private readonly IJobRunService _jobRunService = jobRunService ?? throw new ArgumentNullException(nameof(jobRunService));

    public async Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        var packageRunModel = await _jobRunService.GetPackageRun(packageName, configuration["ConsoleAppPackageRunTempPath"] ?? AppDomain.CurrentDomain.BaseDirectory).ConfigureAwait(false);
        if (packageRunModel == null)
        {
            throw new InvalidOperationException($"Console app package '{packageName}' not found");
        }

        try
        {
            await packageRunModel.ExtractPackage().ConfigureAwait(false);

            var processStartInfo = new ProcessStartInfo
            {
                WorkingDirectory = packageRunModel.PackageRunDirectory,
                FileName = packageRunModel.GetRunFilePath(),
                Arguments = packageRunModel.GetRunArguments(arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = processRunnerFactory.CreateNewProcessRunner(processStartInfo))
            {
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
                packageRunModel.DeletePackageRun();
            }
            catch
            {
                // Empty
            }
        }
    }

    private async Task ProcessOutputDataHandler(string jobRunId, string? data, bool isError, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        if (isError)
        {
            await _jobRunService.InsertJobRunLog(jobRunId, data, isError, cancellationToken);
            return;
        }

        var consoleMessage = ConsoleMessageReader.ReadMessage(data);
        if (consoleMessage == null)
        {
            return;
        }

        await consoleMessageProcessorManager.ProcessMessage(jobRunId, consoleMessage, cancellationToken);
    }
}