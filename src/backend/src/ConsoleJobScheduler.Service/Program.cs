
using ConsoleJobScheduler.Service;
using System.Reflection;
using Topshelf;

static string GetContentRootPath()
{
    return Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
}

var contentRootPath = GetContentRootPath();
var config = new ConfigurationBuilder()
    .SetBasePath(contentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#if DEBUG
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var topShelfExitCode = HostFactory.Run(
        hostConfig =>
        {
            hostConfig.Service<ServiceHost>(
                sc =>
                {
                    sc.ConstructUsing(() => new ServiceHost(contentRootPath));
                    sc.WhenStarted(s => s.Start().GetAwaiter().GetResult());
                    sc.WhenStopped(s => s.Stop().GetAwaiter().GetResult());
                });
            hostConfig.EnableServiceRecovery(r =>
            {
                r.OnCrashOnly();

                // First failure
                r.RestartService(0);

                // Second failure
                r.RestartService(1);

                // Corresponds to Subsequent failures: Restart the Service
                r.RestartService(5);
                r.SetResetPeriod(1);
            });
            hostConfig.SetStartTimeout(TimeSpan.FromSeconds(180));
            hostConfig.SetStopTimeout(TimeSpan.FromSeconds(180));
            hostConfig.StartAutomatically();
            hostConfig.RunAsLocalSystem();
            hostConfig.SetServiceName(config["ServiceName"]);
            hostConfig.SetDisplayName("ConsoleJobScheduler");
            hostConfig.SetDescription("ConsoleJobScheduler");
        });

Console.WriteLine($"ConsoleJobScheduler topshelf exit code: {topShelfExitCode}");