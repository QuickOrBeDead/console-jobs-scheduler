namespace ConsoleJobScheduler.Core.Domain.Runner.Model;

public sealed class JobRunLogDetail(string content, bool isError, DateTime createDate)
{
    public string Content { get; private set; } = content;

    public bool IsError { get; private set; } = isError;

    public DateTime CreateDate { get; private set; } = createDate;
}