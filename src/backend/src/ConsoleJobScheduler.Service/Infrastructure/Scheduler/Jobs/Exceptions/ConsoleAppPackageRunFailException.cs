namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Exceptions;

public sealed class ConsoleAppPackageRunFailException : Exception
{
    public int ExitCode { get; }

    public ConsoleAppPackageRunFailException(int exitCode)
        : this("Console app package run failed", exitCode)
    {
    }

    public ConsoleAppPackageRunFailException(string? message, int exitCode, Exception? innerException)
        : base(message, innerException)
    {
        ExitCode = exitCode;
    }

    public ConsoleAppPackageRunFailException(string? message, int exitCode)
        : base(message)
    {
        ExitCode = exitCode;
    }
}