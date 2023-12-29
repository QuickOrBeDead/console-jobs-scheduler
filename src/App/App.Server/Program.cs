using ConsoleJobScheduler.Core;

var host = new ServiceHost();
await host.Start().ConfigureAwait(false);