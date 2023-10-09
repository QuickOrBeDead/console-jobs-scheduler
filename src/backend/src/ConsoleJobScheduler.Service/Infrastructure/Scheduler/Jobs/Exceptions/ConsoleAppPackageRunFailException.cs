namespace ConsoleJobScheduler.Service.Infrastructure.Scheduler.Jobs.Exceptions;

using System.Runtime.Serialization;

public class ConsoleAppPackageRunFailException : Exception
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

    protected ConsoleAppPackageRunFailException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ExitCode = info.GetInt32("ExitCode");
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("ExitCode", ExitCode, typeof(int));
    }
}