﻿namespace ConsoleJobScheduler.Service.Api.Hubs.Handlers;

using System.Web;

using ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Events;

using MessagePipe;

using Microsoft.AspNetCore.SignalR;

public sealed class JobConsoleLogMessageToHubHandler : IAsyncMessageHandler<JobConsoleLogMessageEvent>
{
    private readonly IHubContext<JobRunConsoleHub> _hubContext;

    public JobConsoleLogMessageToHubHandler(IHubContext<JobRunConsoleHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async ValueTask HandleAsync(JobConsoleLogMessageEvent message, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(message.JobRunId).SendAsync(
            "ReceiveJobConsoleLogMessage",
            HttpUtility.HtmlEncode(message.Data),
            message.IsError,
            cancellationToken: cancellationToken);
    }
}