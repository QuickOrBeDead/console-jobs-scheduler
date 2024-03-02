using System.Diagnostics;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;

namespace ConsoleJobScheduler.Core.Tests.Jobs.Fakes;

public sealed class FakeProcessRunnerFactory(Func<ProcessStartInfo, FakeProcessRunner> fakeProcessRunnerFunc)
    : ProcessRunnerFactory
{
    public override IProcessRunner CreateNewProcessRunner(ProcessStartInfo processStartInfo)
    {
        return fakeProcessRunnerFunc(processStartInfo);
    }
}