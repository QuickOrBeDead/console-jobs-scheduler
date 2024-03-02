using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner.Exceptions;
using ConsoleJobScheduler.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IConsoleAppPackageRunner
{
    Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken);
}

public sealed class ConsoleAppPackageRunner(
    IServiceProvider serviceProvider,
    IProcessRunnerFactory processRunnerFactory,
    IConfiguration configuration)
    : IConsoleAppPackageRunner
{
    private int _messageOrder;

    public async Task Run(string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var packageRunModel = await scope.ServiceProvider.GetRequiredService<IJobRunService>().GetPackageRun(packageName, configuration["ConsoleAppPackageRunTempPath"] ?? AppDomain.CurrentDomain.BaseDirectory).ConfigureAwait(false);
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
                using var countdown = new CountdownEvent(1);
                process.OutputDataReceived += async (_, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    IgnoreObjectDisposedException(() => countdown.AddCount());

                    try
                    {
                        await ProcessOutputDataHandler(jobRunId, GetNextOrder(), e.Data, false, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        IgnoreObjectDisposedException(() => countdown.Signal());
                    }
                };
                process.ErrorDataReceived += async (_, e) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    IgnoreObjectDisposedException(() => countdown.AddCount());

                    try
                    {
                        await ProcessOutputDataHandler(jobRunId, GetNextOrder(), e.Data, true, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        IgnoreObjectDisposedException(() => countdown.Signal());
                    }
                };

                if (!process.Start())
                {
                    throw new InvalidOperationException($"Process couldn't be started: {packageRunModel.FileName}. Arguments: '{packageRunModel.GetRunArguments(arguments)}'.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                countdown.Signal();
                countdown.Wait(TimeSpan.FromMinutes(5), cancellationToken);

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

    private void IgnoreObjectDisposedException(Action action)
    {
        try
        {
            action();
        }
        catch (ObjectDisposedException)
        {
           // Empty
        }
    }

    private int GetNextOrder()
    {
        return Interlocked.Increment(ref _messageOrder);
    }

    private async Task ProcessOutputDataHandler(string jobRunId, int order, string? data, bool isError, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(data))
        {
            return;
        }

        if (isError)
        {
            using var scope = serviceProvider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IJobRunService>().InsertJobRunLog(jobRunId, order, data, isError, cancellationToken).ConfigureAwait(false);
            return;
        }

        var consoleMessage = ConsoleMessageReader.ReadMessage(data);
        if (consoleMessage == null)
        {
            return;
        }

        using var processorScope = serviceProvider.CreateScope();
        await processorScope.ServiceProvider.GetRequiredService<IConsoleMessageProcessorManager>().ProcessMessage(jobRunId, order, consoleMessage, cancellationToken).ConfigureAwait(false);
    }
}