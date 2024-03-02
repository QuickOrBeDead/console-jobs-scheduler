using System.Collections;
using System.Diagnostics;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Infra.EMail;
using ConsoleJobScheduler.Core.Jobs;
using ConsoleJobScheduler.Core.Tests.Jobs.Fakes;
using ConsoleJobScheduler.Messaging.Models;
using MessagePipe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Quartz;
using Z.EntityFramework.Extensions;

namespace ConsoleJobScheduler.Core.Tests.Jobs;

[TestFixture]
public sealed class ConsoleAppPackageJobFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EntityFrameworkManager.ContextFactory = _ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<RunnerDbContext>();
            UseUseSqliteDatabase(optionsBuilder);
            return new RunnerDbContext(optionsBuilder.Options);
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EntityFrameworkManager.ContextFactory = null;
    }
    
    [Test]
    public async Task ShouldExecute()
    {
        // Arrange
        var package = "package";
        var packageParameters = "package-parameters";

        var manualResetEvent = new ManualResetEvent(false);
        FakeProcessRunner? fakeProcessRunner = null;
        Func<ProcessStartInfo, FakeProcessRunner> fakeProcessRunnerFunc = x =>
        {
            fakeProcessRunner = new FakeProcessRunner(x);
            manualResetEvent.Set();
            return fakeProcessRunner;
        };
        var serviceProvider = await CreateServiceProvider(fakeProcessRunnerFunc);
        var consoleAppPackageJob = new ConsoleAppPackageJob(serviceProvider, Substitute.For<ILogger<ConsoleAppPackageJob>>());
        var jobExecutionContext = CreateJobExecutionContext(package, packageParameters);
        var jobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();
        await jobApplicationService.SavePackage(package, await ReadPackageData("GithubReadmeStats.zip"));

        // Act
        var executeTask = Task.Run(async () => await consoleAppPackageJob.Execute(jobExecutionContext));
        manualResetEvent.WaitOne();
        fakeProcessRunner!.AddErrorData("error");
        fakeProcessRunner.AddOutputData("test");
        fakeProcessRunner.AddEmailMessage(new EmailMessage
        {
            To = "to@email.com",
            CC = "cc@email.com",
            Bcc = "bcc@email.com",
            Body = "Body",
            Subject = "Subject"
        });
        fakeProcessRunner.AddLogMessage(ConsoleMessageLogType.Info, "Info");
        
        // Assert
        fakeProcessRunner.StopReceivingEvents();
        await executeTask;


    }

    private static IJobExecutionContext CreateJobExecutionContext(string package, string parameters)
    {
        var jobExecutionContext = Substitute.For<IJobExecutionContext>();
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.JobDataMap.Returns(new JobDataMap((IDictionary)new Dictionary<string, object> { { "package", package }, { "parameters", parameters } }));
        jobExecutionContext.JobDetail.Returns(jobDetail);
        return jobExecutionContext;
    }

    private static Task<byte[]> ReadPackageData(string packageName)
    {
        return File.ReadAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "Jobs", "_Data", packageName));
    }
    
    private static async Task<ServiceProvider> CreateServiceProvider(Func<ProcessStartInfo, FakeProcessRunner> fakeProcessRunnerFunc, TimeProvider? timeProvider = null)
    {
        Environment.SetEnvironmentVariable("ConsoleAppPackageRunTempPath", TestContext.CurrentContext.TestDirectory);
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IEmailSender>());
        services.AddMessagePipe(
            x =>
            {
                x.InstanceLifetime = InstanceLifetime.Singleton;
                x.RequestHandlerLifetime = InstanceLifetime.Singleton;
                x.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
                x.EnableAutoRegistration = false;
            });

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddEnvironmentVariables().Build());
        
        var jobRunModule = new JobRunModule(Substitute.For<IConfigurationRoot>());
        jobRunModule.Register(services, UseUseSqliteDatabase);
        services.RemoveAll<IProcessRunnerFactory>();
        services.AddSingleton<IProcessRunnerFactory>(_ => new FakeProcessRunnerFactory(fakeProcessRunnerFunc));

        var jobHistoryModule = new JobHistoryModule(Substitute.For<IConfigurationRoot>());
        jobHistoryModule.Register(services, UseUseSqliteDatabase);
        
        services.AddSingleton(timeProvider ?? new FakeTimeProvider());
        
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await jobHistoryModule.MigrateDb(serviceProvider);
        
        return serviceProvider;
    }
    
    private static void UseUseSqliteDatabase(DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        builder.UseSqlite($"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")}");
    }
}