using System.Web;
using ConsoleJobScheduler.Core.Domain.Runner.Events;
using MessagePipe;

using Microsoft.AspNetCore.SignalR;

namespace ConsoleJobScheduler.Core.Api.Hubs.Handlers;

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