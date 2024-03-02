using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using System.Reflection;
using ConsoleJobScheduler.Messaging;
using ConsoleJobScheduler.Messaging.Models;

namespace ConsoleJobScheduler.Core.Tests.Jobs.Fakes;

public sealed class FakeProcessRunner(ProcessStartInfo processStartInfo) : IProcessRunner
{
    private readonly ManualResetEvent _manualResetEvent = new(false);
    public ProcessStartInfo StartInfo { get; } = processStartInfo;

    public bool Start()
    {
        return true;
    }

    public void BeginOutputReadLine()
    {
    }

    public void BeginErrorReadLine()
    {
    }

    public Task WaitForExitAsync(CancellationToken cancellationToken)
    {
        _manualResetEvent.WaitOne();
        return Task.CompletedTask;
    }

    public void StopReceivingEvents()
    {
        _manualResetEvent.Set();
    }

    public void AddEmailMessage(EmailMessage message)
    {
        AddOutputData(ConsoleMessageWriter.GetEmailMessage(message));
    }
    
    public void AddLogMessage(ConsoleMessageLogType type, string message)
    {
        AddOutputData(ConsoleMessageWriter.GetLogMessage(type, message));
    }
    
    public void AddOutputData(string data)
    {
        OutputDataReceived?.Invoke(this, CreateDataReceivedEventArgs(data));
    }
    
    public void AddErrorData(string data)
    {
        ErrorDataReceived?.Invoke(this, CreateDataReceivedEventArgs(data));
    }

    private static DataReceivedEventArgs CreateDataReceivedEventArgs(string data)
    {
        return (DataReceivedEventArgs)typeof(DataReceivedEventArgs)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, [typeof(string)])!
            .Invoke([data]);
    }

    public int ExitCode => 0;
    
    public event DataReceivedEventHandler? OutputDataReceived;
    public event DataReceivedEventHandler? ErrorDataReceived;
    
    public void Dispose()
    {
    }
}