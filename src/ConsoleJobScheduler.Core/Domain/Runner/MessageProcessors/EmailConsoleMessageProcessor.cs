using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public sealed class EmailConsoleMessageProcessor(IJobApplicationService jobApplicationService)
    : IConsoleMessageProcessor
{
    public ConsoleMessageType MessageType => ConsoleMessageType.Email;

    public Task ProcessMessage(string jobRunId, int messageOrder, object message, CancellationToken cancellationToken = default)
    {
        return jobApplicationService.SendEmailMessage(jobRunId, messageOrder, (EmailMessage)message, cancellationToken);
    }
}