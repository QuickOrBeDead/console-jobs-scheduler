using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public sealed class ConsoleLogMessageProcessor : IConsoleMessageProcessor
{
    private readonly IJobRunService _jobRunService;

    public ConsoleLogMessageProcessor(IJobRunService jobRunService)
    {
        _jobRunService = jobRunService;
    }

    public ConsoleMessageType MessageType => ConsoleMessageType.Log;

    public Task ProcessMessage(string jobRunId, object message, CancellationToken cancellationToken = default)
    {
        return _jobRunService.InsertJobRunLog(jobRunId, ((ConsoleLogMessage)message).Message, false, cancellationToken);
    }
}