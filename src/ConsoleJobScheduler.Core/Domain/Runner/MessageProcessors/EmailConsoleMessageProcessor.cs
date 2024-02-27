using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public sealed class EmailConsoleMessageProcessor : IConsoleMessageProcessor
{
    private readonly IJobApplicationService _jobApplicationService;

    public EmailConsoleMessageProcessor(IJobApplicationService jobApplicationService)
    {
        _jobApplicationService = jobApplicationService;
    }

    public ConsoleMessageType MessageType => ConsoleMessageType.Email;

    public Task ProcessMessage(string jobRunId, object message, CancellationToken cancellationToken = default)
    {
        return _jobApplicationService.SendEmailMessage(jobRunId,  (EmailMessage)message, cancellationToken);
    }
}