using System.Collections;
using System.Diagnostics;
using System.Text;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.History.Model;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
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
using netDumbster.smtp;
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
    public async Task ShouldExecuteConsolePackage()
    {
        // Arrange
        using var simpleSmtpServer = SimpleSmtpServer.Start(44444);
        
        var initialSignalTime = DateTime.UtcNow;

        var jobRunId = "Node1638440532318841376";
        var package = "package";
        var packageParameters = "package-parameters";
        var packageRunTempPath = TestContext.CurrentContext.TestDirectory;

        var manualResetEvent = new ManualResetEvent(false);
        FakeProcessRunner? fakeProcessRunner = null;
        Func<ProcessStartInfo, FakeProcessRunner> fakeProcessRunnerFunc = x =>
        {
            fakeProcessRunner = new FakeProcessRunner(x);
            manualResetEvent.Set();
            return fakeProcessRunner;
        };
        var serviceProvider = await CreateServiceProvider(fakeProcessRunnerFunc, packageRunTempPath);
        var consoleAppPackageJob = new ConsoleAppPackageJob(serviceProvider, Substitute.For<ILogger<ConsoleAppPackageJob>>());
        await SavePackage(serviceProvider, package, "GithubReadmeStats.zip");
        await InsertJobRunHistory(serviceProvider, jobRunId, package, initialSignalTime);

        // Act
        var jobExecutionContext = CreateJobExecutionContext(package, packageParameters, jobRunId);
        var executeTask = Task.Run(async () => await consoleAppPackageJob.Execute(jobExecutionContext));
        manualResetEvent.WaitOne();
        fakeProcessRunner!.WaitForErrorAndOutputReadLine();
        
        Assert.That(fakeProcessRunner.StartInfo.WorkingDirectory, Does.Exist);
        
        fakeProcessRunner.AddErrorData("#1 error");
        fakeProcessRunner.AddOutputData("#2 output");
        var emailMessage = new EmailMessage
        {
            To = "to@email.com",
            CC = "cc@email.com",
            Bcc = "bcc@email.com",
            Body = "Body",
            Subject = "Subject"
        };
        
        var attachmentContent = "Attachment Text";
        emailMessage.AddAttachment("attachment.txt", "text/plain", Encoding.UTF8.GetBytes(attachmentContent));
        fakeProcessRunner.AddEmailMessage(emailMessage);
        fakeProcessRunner.AddLogMessage(ConsoleMessageLogType.Info, "#3 info");
        fakeProcessRunner.StopReceivingEvents();
        await executeTask;
        
        // Assert
        Assert.That(fakeProcessRunner.StartInfo.WorkingDirectory, Does.Not.Exist);
        
        Assert.That(fakeProcessRunner.StartInfo.UseShellExecute, Is.False);
        Assert.That(fakeProcessRunner.StartInfo.RedirectStandardOutput, Is.True);
        Assert.That(fakeProcessRunner.StartInfo.RedirectStandardError, Is.True);
        Assert.That(fakeProcessRunner.StartInfo.FileName, Is.EqualTo("dotnet"));
        Assert.That(fakeProcessRunner.StartInfo.Arguments, Is.EqualTo($"GithubReadmeStats.dll {packageParameters}"));
        Assert.That(fakeProcessRunner.StartInfo.WorkingDirectory, Does.StartWith(Path.Combine(packageRunTempPath, "Temp", package)));

        var jobHistoryExecutionDetail = await GetJobHistoryExecutionDetail(serviceProvider, jobRunId);
        Assert.That(jobHistoryExecutionDetail, Is.Not.Null);
        Assert.That(jobHistoryExecutionDetail!.LastSignalTime, Is.GreaterThan(initialSignalTime));
        
        var jobExecutionDetail = await GetJobExecutionDetail(serviceProvider, jobRunId); 
        Assert.That(jobExecutionDetail, Is.Not.Null);
        Assert.That(jobExecutionDetail!.Attachments.Count, Is.EqualTo(1));
        Assert.That(jobExecutionDetail.Attachments[0].FileName, Is.EqualTo("attachment.txt"));
        Assert.That(jobExecutionDetail.Logs.Count, Is.EqualTo(5));
        Assert.That(jobExecutionDetail.Logs[0].Content, Is.EqualTo("##[error] #1 error"));
        Assert.That(jobExecutionDetail.Logs[0].IsError, Is.True);
        Assert.That(jobExecutionDetail.Logs[1].Content, Is.EqualTo("#2 output"));
        Assert.That(jobExecutionDetail.Logs[1].IsError, Is.False);
        Assert.That(jobExecutionDetail.Logs[2].Content, Is.EqualTo("Sending email to to@email.com"));
        Assert.That(jobExecutionDetail.Logs[2].IsError, Is.False);
        Assert.That(jobExecutionDetail.Logs[3].Content, Is.EqualTo("Email is sent to to@email.com"));
        Assert.That(jobExecutionDetail.Logs[3].IsError, Is.False);
        Assert.That(jobExecutionDetail.Logs[4].Content, Is.EqualTo("#3 info"));
        Assert.That(jobExecutionDetail.Logs[4].IsError, Is.False);
        
        Assert.That(simpleSmtpServer.ReceivedEmailCount, Is.EqualTo(1));
        Assert.That(simpleSmtpServer.ReceivedEmail.Length, Is.EqualTo(1));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].FromAddress.Address, Is.EqualTo("from@email.com"));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].Subject, Is.EqualTo(emailMessage.Subject));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].ToAddresses.Length, Is.EqualTo(3));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].ToAddresses[0].Address, Is.EqualTo(emailMessage.To));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].ToAddresses[1].Address, Is.EqualTo(emailMessage.CC));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].ToAddresses[2].Address, Is.EqualTo(emailMessage.Bcc));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].MessageParts[0].BodyData, Is.EqualTo(emailMessage.Body));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].MessageParts[1].BodyData, Is.EqualTo(attachmentContent));
        Assert.That(simpleSmtpServer.ReceivedEmail[0].MessageParts[1].HeaderData, Does.Match("Content-Type: text/plain; name=attachment.txt\nContent-Transfer-Encoding: Base64\nContent-ID: [0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}\nContent-Disposition: attachment\n"));
    }

    private static async Task<JobExecutionDetailModel?> GetJobExecutionDetail(IServiceProvider serviceProvider, string jobRunId)
    {
        using var scope = serviceProvider.CreateScope();
        var jobApplicationService = scope.ServiceProvider.GetRequiredService<IJobApplicationService>();
        return await jobApplicationService.GetJobExecutionDetail(jobRunId);
    }
    
    private static async Task<JobExecutionHistoryDetail?> GetJobHistoryExecutionDetail(IServiceProvider serviceProvider, string jobRunId)
    {
        using var scope = serviceProvider.CreateScope();
        var jobHistoryApplicationService = scope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>();
        return await jobHistoryApplicationService.GetJobExecutionDetail(jobRunId);
    }

    private static async Task InsertJobRunHistory(IServiceProvider serviceProvider, string jobRunId, string package, DateTime lastSignalTime)
    {
        using var scope = serviceProvider.CreateScope();
        var jobHistoryApplicationService = scope.ServiceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(new JobExecutionHistory(jobRunId, "Test", "Test",
            package, "job", "jobGroup", "trigger", "triggerGroup", DateTime.UtcNow, DateTime.UtcNow, lastSignalTime,
            DateTime.UtcNow, "0 0/1 * 1/1 * ? *"));
    }

    private static async Task SavePackage(IServiceProvider serviceProvider, string package, string packageZipName)
    {
        using var scope = serviceProvider.CreateScope();
        var jobApplicationService = scope.ServiceProvider.GetRequiredService<IJobApplicationService>();
        await jobApplicationService.SavePackage(package, await ReadPackageData(packageZipName));
    }

    private static IJobExecutionContext CreateJobExecutionContext(string package, string parameters, string jobRunId)
    {
        var jobExecutionContext = Substitute.For<IJobExecutionContext>();
        var jobDetail = Substitute.For<IJobDetail>();
        jobDetail.JobDataMap.Returns(new JobDataMap((IDictionary)new Dictionary<string, object> { { "package", package }, { "parameters", parameters } }));
        jobExecutionContext.JobDetail.Returns(jobDetail);
        jobExecutionContext.FireInstanceId.Returns(jobRunId);
        return jobExecutionContext;
    }

    private static Task<byte[]> ReadPackageData(string packageZipName)
    {
        return File.ReadAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "Jobs", "_Data", packageZipName));
    }
    
    private static async Task<ServiceProvider> CreateServiceProvider(Func<ProcessStartInfo, FakeProcessRunner> fakeProcessRunnerFunc, string packageRunTempPath, TimeProvider? timeProvider = null)
    {
        Environment.SetEnvironmentVariable("ConsoleAppPackageRunTempPath", packageRunTempPath);
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
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

        var settingsModule = new SettingsModule(Substitute.For<IConfigurationRoot>());
        settingsModule.Register(services, UseUseSqliteDatabase);
        
        services.AddSingleton(timeProvider ?? new FakeTimeProvider());
        
        var serviceProvider = services.BuildServiceProvider();

        using var createDbScope = serviceProvider.CreateScope();
        await using var dbContext = createDbScope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await jobHistoryModule.MigrateDb(serviceProvider);
        await settingsModule.MigrateDb(serviceProvider);
        
        using var scope = serviceProvider.CreateScope();
        var settingsApplicationService = scope.ServiceProvider.GetRequiredService<ISettingsApplicationService>();
        await settingsApplicationService.SaveSettings(new SmtpSettings
        {
            Host = "localhost",
            Port = 44444,
            From = "from@email.com",
            FromName = "from",
            Domain = string.Empty,
            UserName = "test",
            Password = "test",
            EnableSsl = false
        });
        
        return serviceProvider;
    }
    
    private static void UseUseSqliteDatabase(DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        builder.UseSqlite($"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")}");
    }
}