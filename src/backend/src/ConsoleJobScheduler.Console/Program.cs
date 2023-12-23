using System.Reflection;

using ConsoleJobScheduler.Service;

var contentRootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
var host = new ServiceHost(contentRootPath);
await host.Start().ConfigureAwait(false);