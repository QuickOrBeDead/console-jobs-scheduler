namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Exceptions;

using System.Diagnostics.CodeAnalysis;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
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