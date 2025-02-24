namespace ConsoleJobScheduler.Core.Domain.Runner.Events;

public sealed class JobConsoleLogMessageEvent
{
    public string JobRunId { get; }

    public string Data { get; }

    public bool IsError { get; }

    public JobConsoleLogMessageEvent(string jobRunId, string data, bool isError)
    {
        JobRunId = jobRunId;
        Data = data;
        IsError = isError;
    }
}