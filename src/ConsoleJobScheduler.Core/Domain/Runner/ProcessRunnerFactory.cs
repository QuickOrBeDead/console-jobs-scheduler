using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;

namespace ConsoleJobScheduler.Core.Domain.Runner;

public interface IProcessRunnerFactory
{
    IProcessRunner CreateNewProcessRunner(ProcessStartInfo processStartInfo);
}

public class ProcessRunnerFactory : IProcessRunnerFactory
{
    public virtual IProcessRunner CreateNewProcessRunner(ProcessStartInfo processStartInfo)
    {
        return new ProcessRunner(processStartInfo);
    }
}