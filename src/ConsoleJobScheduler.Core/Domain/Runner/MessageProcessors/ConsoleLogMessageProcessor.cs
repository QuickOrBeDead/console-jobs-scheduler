using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public sealed class ConsoleLogMessageProcessor : IConsoleMessageProcessor
{
    private readonly IJobApplicationService _jobApplicationService;

    public ConsoleLogMessageProcessor(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    public ConsoleMessageType MessageType => ConsoleMessageType.Log;

    public Task ProcessMessage(string jobRunId, object message, CancellationToken cancellationToken = default)
    {
        return _jobApplicationService.InsertJobRunLog(jobRunId, ((ConsoleLogMessage)message).Message, false, cancellationToken);
    }
}