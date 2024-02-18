using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IConsoleMessageProcessor
{
    ConsoleMessageType MessageType { get; }

    Task ProcessMessage(string jobRunId, object message, CancellationToken cancellationToken = default);
}