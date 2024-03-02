using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IConsoleMessageProcessorManager
{
    Task ProcessMessage(string jobRunId, ConsoleMessage message, CancellationToken cancellationToken = default);
}

public sealed class ConsoleMessageProcessorManager : IConsoleMessageProcessorManager
{
    private readonly IDictionary<ConsoleMessageType, IConsoleMessageProcessor> _processors = new Dictionary<ConsoleMessageType, IConsoleMessageProcessor>();

    public ConsoleMessageProcessorManager(IEnumerable<IConsoleMessageProcessor> messageProcessors)
    {
        ArgumentNullException.ThrowIfNull(messageProcessors);

        foreach (var messageProcessor in messageProcessors)
        {
            _processors[messageProcessor.MessageType] = messageProcessor;
        }
    }

    public async Task ProcessMessage(string jobRunId, ConsoleMessage message, CancellationToken cancellationToken = default)
    {
        if (_processors.TryGetValue(message.MessageType, out var processor))
        {
            if (message.Message == null)
            {
                // TODO: log
                return;
            }

            await processor.ProcessMessage(jobRunId, message.Message, cancellationToken).ConfigureAwait(false);

            return;
        }

        // TODO: log
    }
}