using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner.MessageProcessors;

public interface IConsoleMessageProcessor
{
    ConsoleMessageType MessageType { get; }

    Task ProcessMessage(string jobRunId, int messageOrder, object message, CancellationToken cancellationToken = default);
}