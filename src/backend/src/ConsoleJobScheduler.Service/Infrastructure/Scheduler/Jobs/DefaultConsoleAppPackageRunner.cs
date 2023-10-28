﻿namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs;

using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;

using ConsoleJobScheduler.Messaging;
using ConsoleJobScheduler.Messaging.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Plugins;

using Events;
using Exceptions;

using MessagePipe;

public sealed class DefaultConsoleAppPackageRunner : IConsoleAppPackageRunner
{
    private readonly IAsyncPublisher<JobConsoleLogMessageEvent> _jobConsoleLogMessagePublisher;
    private readonly IPackageStorage _packageStorage;
    private readonly IEmailSender _emailSender;
    private readonly ConsoleMessageReader _consoleMessageReader = new();

    private readonly string _tempRootPath;

    public DefaultConsoleAppPackageRunner(
        IAsyncPublisher<JobConsoleLogMessageEvent> jobConsoleLogMessagePublisher, 
        IPackageStorage packageStorage,
        IEmailSender emailSender,
        string tempRootPath)
    {
        if (string.IsNullOrWhiteSpace(tempRootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tempRootPath));
        }

        _jobConsoleLogMessagePublisher = jobConsoleLogMessagePublisher ?? throw new ArgumentNullException(nameof(jobConsoleLogMessagePublisher));
        _packageStorage = packageStorage ?? throw new ArgumentNullException(nameof(packageStorage));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _tempRootPath = tempRootPath;
    }

    public async Task Run(IJobHistoryDelegate jobHistoryDelegate, string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        var tempDirectory = Path.Combine(_tempRootPath, "Temp");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        var runDirectory = Path.Combine(tempDirectory, packageName, Guid.NewGuid().ToString("N"));

        try
        {
            using (var stream = _packageStorage.GetPackageStream(packageName))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                {
                    zipArchive.ExtractToDirectory(runDirectory);
                }
            }

            using (var process = new Process())
            {
                var pathToExecutable = Path.Combine(runDirectory, $"{packageName}{(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? string.Empty : ".exe")}");

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = pathToExecutable;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Arguments = arguments;

                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += async (_, e) => await ProcessOutputDataHandler(jobHistoryDelegate, jobRunId, e.Data, false);
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += async (_, e) => await ProcessOutputDataHandler(jobHistoryDelegate, jobRunId, e.Data, true);

                if (!process.Start())
                {
                    throw new InvalidOperationException($"Process couldn't be started: {pathToExecutable}");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
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
                var directory = new DirectoryInfo(runDirectory);
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

    private async Task ProcessOutputDataHandler(IJobHistoryDelegate jobHistoryDelegate, string jobRunId, string? data, bool isError)
    {
        if (!string.IsNullOrEmpty(data))
        {
            if (isError)
            {
                await jobHistoryDelegate.InsertJobRunLog(jobRunId, data, isError);
                await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, data, isError));
            }
            else
            {
                var consoleMessage = _consoleMessageReader.ReadMessage(data);
                if (consoleMessage != null)
                {
                    if (consoleMessage.MessageType == ConsoleMessageType.Email)
                    {
                        var emailMessage = (EmailMessage)consoleMessage.Message;

                        await jobHistoryDelegate.InsertJobRunLog(jobRunId, $"Sending email to ${emailMessage.To}", false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Sending email to {emailMessage.To}", false));
                        await _emailSender.SendMailAsync(emailMessage);
                        await jobHistoryDelegate.InsertJobRunLog(jobRunId, $"Email is sent to ${emailMessage.To}", false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Email is sent to {emailMessage.To}", false));
                    }
                    else if (consoleMessage.MessageType == ConsoleMessageType.Log)
                    {
                        var logMessage = (ConsoleLogMessage)consoleMessage.Message;
                        await jobHistoryDelegate.InsertJobRunLog(jobRunId, logMessage.Message, false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, logMessage.Message, isError));
                    }
                }
            }
        }
    }
}