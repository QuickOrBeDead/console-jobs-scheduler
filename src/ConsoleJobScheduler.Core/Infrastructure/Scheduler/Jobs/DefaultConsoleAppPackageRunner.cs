﻿using System.Diagnostics;
using System.IO.Compression;

using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Events;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Exceptions;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Models;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins;
using ConsoleJobScheduler.Messaging;
using ConsoleJobScheduler.Messaging.Models;

using MessagePipe;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs;

public sealed class DefaultConsoleAppPackageRunner : IConsoleAppPackageRunner
{
    private readonly IAsyncPublisher<JobConsoleLogMessageEvent> _jobConsoleLogMessagePublisher;
    private readonly IEmailSender _emailSender;

    private readonly string _tempRootPath;

    public DefaultConsoleAppPackageRunner(
        IAsyncPublisher<JobConsoleLogMessageEvent> jobConsoleLogMessagePublisher,
        IEmailSender emailSender,
        string tempRootPath)
    {
        if (string.IsNullOrWhiteSpace(tempRootPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tempRootPath));
        }

        _jobConsoleLogMessagePublisher = jobConsoleLogMessagePublisher ?? throw new ArgumentNullException(nameof(jobConsoleLogMessagePublisher));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _tempRootPath = tempRootPath;
    }

    public async Task Run(IJobStoreDelegate jobStoreDelegate, string jobRunId, string packageName, string arguments, CancellationToken cancellationToken)
    {
        var tempDirectory = Path.Combine(_tempRootPath, "Temp");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        var runDirectory = Path.Combine(tempDirectory, packageName, Guid.NewGuid().ToString("N"));

        try
        {
            var packageRunModel = await jobStoreDelegate.GetPackageRun(packageName).ConfigureAwait(false);
            if (packageRunModel == null)
            {
                throw new InvalidOperationException($"Console app package '{packageName}' not found");
            }

            using (var stream = new MemoryStream(packageRunModel.Content))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, false))
                {
                    zipArchive.ExtractToDirectory(runDirectory);
                }
            }

            using (var process = new Process())
            {
                var startInfoArguments = string.IsNullOrWhiteSpace(packageRunModel.Arguments)
                    ? arguments
                    : $"{packageRunModel.Arguments} {arguments}";

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WorkingDirectory = runDirectory;
                process.StartInfo.FileName = packageRunModel.FileName.StartsWith("./", StringComparison.OrdinalIgnoreCase) ? Path.Combine(runDirectory, packageRunModel.FileName) : packageRunModel.FileName;
                process.StartInfo.Arguments = startInfoArguments;
                process.StartInfo.UseShellExecute = false;

                var processOutputTasks = new List<Task>();

                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += async (_, e) =>
                {
                    var task = ProcessOutputDataHandler(jobStoreDelegate, jobRunId, e.Data, false, cancellationToken);
                    processOutputTasks.Add(task);
                    await task;
                };
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += async (_, e) =>
                {
                    var task = ProcessOutputDataHandler(jobStoreDelegate, jobRunId, e.Data, true, cancellationToken);
                    processOutputTasks.Add(task);
                    await task;
                };

                if (!process.Start())
                {
                    throw new InvalidOperationException($"Process couldn't be started: {packageRunModel.FileName}. Arguments: '{startInfoArguments}'.");
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

    private async Task ProcessOutputDataHandler(IJobStoreDelegate jobStoreDelegate, string jobRunId, string? data, bool isError, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(data))
        {
            if (isError)
            {
                await jobStoreDelegate.InsertJobRunLog(jobRunId, data, isError, cancellationToken);
                await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, data, isError), cancellationToken);
            }
            else
            {
                var consoleMessage = ConsoleMessageReader.ReadMessage(data);
                if (consoleMessage != null)
                {
                    if (consoleMessage.MessageType == ConsoleMessageType.Email)
                    {
                        var emailMessage = (EmailMessage)consoleMessage.Message;

                        await jobStoreDelegate.InsertJobRunLog(jobRunId, $"Sending email to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Sending email to {emailMessage.To}", false), cancellationToken).ConfigureAwait(false);
                        var emailModel = new EmailModel
                        {
                            Subject = emailMessage.Subject,
                            Body = emailMessage.Body,
                            To = emailMessage.To,
                            CC = emailMessage.CC,
                            Bcc = emailMessage.Bcc,
                            JobRunId = jobRunId,
                            Attachments = emailMessage.Attachments.Select(x => new AttachmentModel
                            {
                                JobRunId = jobRunId,
                                FileName = x.FileName,
                                ContentType = x.ContentType,
                                FileContent = x.FileContent
                            }).ToList()
                        };
                        await jobStoreDelegate.InsertJobRunEmail(emailModel, cancellationToken).ConfigureAwait(false);
                        await _emailSender.SendMailAsync(emailMessage, cancellationToken).ConfigureAwait(false);
                        await jobStoreDelegate.UpdateJobRunEmailIsSent(emailModel.Id, true, cancellationToken).ConfigureAwait(false);
                        await jobStoreDelegate.InsertJobRunLog(jobRunId, $"Email is sent to {emailMessage.To}", false, cancellationToken).ConfigureAwait(false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, $"Email is sent to {emailMessage.To}", false), cancellationToken);
                    }
                    else if (consoleMessage.MessageType == ConsoleMessageType.Log)
                    {
                        var logMessage = (ConsoleLogMessage)consoleMessage.Message;
                        await jobStoreDelegate.InsertJobRunLog(jobRunId, logMessage.Message, false, cancellationToken).ConfigureAwait(false);
                        await _jobConsoleLogMessagePublisher.PublishAsync(new JobConsoleLogMessageEvent(jobRunId, logMessage.Message, isError), cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}