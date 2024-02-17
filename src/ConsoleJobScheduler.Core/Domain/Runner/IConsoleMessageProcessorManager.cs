using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Domain.Runner
{
    public interface IConsoleMessageProcessorManager
    {
        Task ProcessMessage(string jobRunId, ConsoleMessage message, CancellationToken cancellationToken = default);
    }
}