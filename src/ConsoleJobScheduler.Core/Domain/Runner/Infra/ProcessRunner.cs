using System.Diagnostics;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra
{
    public interface IProcessRunner : IDisposable
    {
        bool Start();

        void BeginOutputReadLine();

        void BeginErrorReadLine();

        Task WaitForExitAsync(CancellationToken cancellationToken);

        int ExitCode { get; }

        event DataReceivedEventHandler? OutputDataReceived;

        event DataReceivedEventHandler? ErrorDataReceived;
    }

    public sealed class ProcessRunner(ProcessStartInfo processStartInfo) : IProcessRunner
    {
        private readonly Process _process = new() { StartInfo = processStartInfo ?? throw new ArgumentNullException(nameof(processStartInfo)) };

        public event DataReceivedEventHandler? OutputDataReceived
        {
            add
            {
                _process.OutputDataReceived += value;
            }
            remove
            {
                _process.OutputDataReceived -= value;
            }
        }

        public event DataReceivedEventHandler? ErrorDataReceived
        {
            add
            {
                _process.ErrorDataReceived += value;
            }
            remove
            {
                _process.ErrorDataReceived -= value;
            }
        }

        public bool Start()
        {
            return _process.Start();
        }

        public void BeginOutputReadLine()
        {
            _process.BeginOutputReadLine();
        }

        public void BeginErrorReadLine()
        {
            _process.BeginErrorReadLine();
        }

        public Task WaitForExitAsync(CancellationToken cancellationToken)
        {
            return _process.WaitForExitAsync(cancellationToken);
        }

        public int ExitCode => _process.ExitCode;

        ~ProcessRunner()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            _process.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}