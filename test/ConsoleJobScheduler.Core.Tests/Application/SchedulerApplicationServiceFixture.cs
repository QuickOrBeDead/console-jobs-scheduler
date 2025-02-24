using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.Runner;
using ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;
using ConsoleJobScheduler.Core.Domain.Scheduler.Model;
using ConsoleJobScheduler.Core.Infra.EMail;
using ConsoleJobScheduler.Core.Tests.Application.Fakes;
using MessagePipe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Z.EntityFramework.Extensions;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class SchedulerApplicationServiceFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EntityFrameworkManager.ContextFactory = _ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<HistoryDbContext>();
            UseUseSqliteDatabase(optionsBuilder);
            return new HistoryDbContext(optionsBuilder.Options);
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EntityFrameworkManager.ContextFactory = null;
    }

    [Test]
    public async Task ShouldRunJobWhenJobAdded()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeSchedulerApplicationService = serviceProvider.GetRequiredService<ISchedulerApplicationService>();
        var schedulerManager = serviceProvider.GetRequiredService<ISchedulerManager>();
        var fakeConsoleAppPackageRunner = (FakeConsoleAppPackageRunner)serviceProvider.GetRequiredService<IConsoleAppPackageRunner>();

        var job = new JobAddOrUpdateModel
        {
            JobName = "test",
            Description = "test job",
            JobGroup = "TestJobs",
            CronExpression = "0/1 0/1 * 1/1 * ? *",
            Package = "TestPackage",
            Parameters = "TestPackageParameters"
        };
        
        await schedulerManager.Start();
        
        // Act
        var addTask = Task.Run(async () => await arrangeSchedulerApplicationService.AddOrUpdateJob(job));
        fakeConsoleAppPackageRunner.WaitForCompletion(TimeSpan.FromSeconds(30));
        await addTask;
        
        // Assert
        var schedulerApplicationService = serviceProvider.GetRequiredService<ISchedulerApplicationService>();
        var jobs = await schedulerApplicationService.ListJobs();
        var jobDetail = await schedulerApplicationService.GetJobDetail(job.JobGroup, job.JobName);
        var statistics = await schedulerApplicationService.GetStatistics();
        
        await schedulerManager.Shutdown();
        
        Assert.That(statistics.Metadata, Is.Not.Null);
        Assert.That(statistics.Metadata.Shutdown, Is.False);
        Assert.That(statistics.Metadata.SchedulerName, Is.EqualTo(GetSchedulerInstanceName()));
        Assert.That(statistics.Metadata.SchedulerInstanceId, Is.EqualTo(GetSchedulerInstanceId()));
        Assert.That(statistics.Metadata.Started, Is.True);
        Assert.That(statistics.Metadata.Summary, Is.Not.Null);
        Assert.That(statistics.Metadata.Version, Is.Not.Null);
        Assert.That(statistics.Metadata.RunningSince, Is.Not.Null);
        Assert.That(statistics.Metadata.SchedulerRemote, Is.False);
        Assert.That(statistics.Metadata.SchedulerType, Is.EqualTo("Quartz.Impl.StdScheduler"));
        Assert.That(statistics.Metadata.InStandbyMode, Is.False);
        Assert.That(statistics.Metadata.JobStoreClustered, Is.False);
        Assert.That(statistics.Metadata.JobStoreType, Is.EqualTo("CustomJobStoreTx"));
        Assert.That(statistics.Metadata.ThreadPoolSize, Is.EqualTo(100));
        Assert.That(statistics.Metadata.ThreadPoolType, Is.EqualTo("Quartz.Simpl.DefaultThreadPool"));
        Assert.That(statistics.Metadata.JobStoreSupportsPersistence, Is.True);
        Assert.That(statistics.Nodes.Count, Is.EqualTo(0));
        
        Assert.That(fakeConsoleAppPackageRunner.JobRunId, Is.Not.Null);
        Assert.That(fakeConsoleAppPackageRunner.PackageName, Is.EqualTo(job.Package));
        Assert.That(fakeConsoleAppPackageRunner.Arguments, Is.EqualTo(job.Parameters));

        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var jobExecutionDetail = await jobHistoryApplicationService.GetJobExecutionDetail(fakeConsoleAppPackageRunner.JobRunId!);
        
        Assert.That(jobExecutionDetail, Is.Not.Null);
        Assert.That(jobExecutionDetail!.Id, Is.EqualTo(fakeConsoleAppPackageRunner.JobRunId));
        Assert.That(jobExecutionDetail.InstanceName, Is.Not.Null);
        Assert.That(jobExecutionDetail.PackageName, Is.EqualTo(job.Package));
        Assert.That(jobExecutionDetail.TriggerGroup, Is.EqualTo("TestJobs.test"));
        Assert.That(jobExecutionDetail.TriggerName, Is.EqualTo("TestJobs.test"));
        Assert.That(jobExecutionDetail.JobName, Is.EqualTo(job.JobName));
        Assert.That(jobExecutionDetail.JobGroup, Is.EqualTo(job.JobGroup));
        Assert.That(jobExecutionDetail.ScheduledTime, Is.Not.Null);
        Assert.That(jobExecutionDetail.FiredTime, Is.GreaterThan(default(DateTime)));
        Assert.That(jobExecutionDetail.RunTime, Is.Not.Null);
        Assert.That(jobExecutionDetail.Completed, Is.EqualTo(true));
        Assert.That(jobExecutionDetail.Vetoed, Is.EqualTo(false));
        Assert.That(jobExecutionDetail.HasError, Is.EqualTo(false));
        Assert.That(jobExecutionDetail.LastSignalTime, Is.GreaterThan(default(DateTime)));
        Assert.That(jobExecutionDetail.ErrorMessage, Is.Null);
        Assert.That(jobExecutionDetail.CronExpressionDescription, Is.EqualTo("Every second"));
        
        Assert.That(jobs.TotalCount, Is.EqualTo(1));
        Assert.That(jobs.Items.Count(), Is.EqualTo(1));
        Assert.That(jobs.Items.First().JobName, Is.EqualTo(job.JobName));
        Assert.That(jobs.Items.First().JobGroup, Is.EqualTo(job.JobGroup));
        Assert.That(jobs.Items.First().JobType, Is.EqualTo("ConsoleAppPackageJob"));
        Assert.That(jobs.Items.First().TriggerDescription, Is.EqualTo("Every second"));
        Assert.That(jobs.Items.First().LastFireTime, Is.Not.Null);
        Assert.That(jobs.Items.First().NextFireTime, Is.Not.Null);
        
        Assert.That(jobDetail, Is.Not.Null);
        Assert.That(jobDetail!.JobName, Is.EqualTo(job.JobName));
        Assert.That(jobDetail.JobGroup, Is.EqualTo(job.JobGroup));
        Assert.That(jobDetail.Description, Is.EqualTo(job.Description));
        Assert.That(jobDetail.CronExpression, Is.EqualTo(job.CronExpression));
        Assert.That(jobDetail.Parameters, Is.EqualTo(job.Parameters));
        Assert.That(jobDetail.Package, Is.EqualTo(job.Package));
        Assert.That(jobDetail.CronExpressionDescription, Is.EqualTo("Every second"));
    }
    
    private static async Task<ServiceProvider> CreateServiceProvider()
    {
        Environment.SetEnvironmentVariable("ConnectionString", GetConnectionString());
        Environment.SetEnvironmentVariable("TablePrefix", "qrtz_");
        Environment.SetEnvironmentVariable("SchedulerInstanceId", GetSchedulerInstanceId());
        Environment.SetEnvironmentVariable("SchedulerInstanceName", GetSchedulerInstanceName());
        
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        
        var schedulerModule = new SchedulerModule(configuration, SchedulerDbType.SqLite);
        schedulerModule.Register(services);
        
        var jobHistoryModule = new JobHistoryModule(configuration);
        jobHistoryModule.Register(services, UseUseSqliteDatabase);
        
        services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        services.AddSingleton(Substitute.For<IEmailSender>());
        services.AddMessagePipe(
            x =>
            {
                x.InstanceLifetime = InstanceLifetime.Singleton;
                x.RequestHandlerLifetime = InstanceLifetime.Singleton;
                x.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
                x.EnableAutoRegistration = false;
            });
        
        var jobRunModule = new JobRunModule(configuration);
        jobRunModule.Register(services, UseUseSqliteDatabase);

        var settingsModule = new SettingsModule(configuration);
        settingsModule.Register(services, UseUseSqliteDatabase);

        services.AddSingleton<IConsoleAppPackageRunner, FakeConsoleAppPackageRunner>();

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        await jobHistoryModule.MigrateDb(serviceProvider);
        await jobRunModule.MigrateDb(serviceProvider);
        await settingsModule.MigrateDb(serviceProvider);
        schedulerModule.MigrateDb();
        
        return serviceProvider;
    }

    private static string GetSchedulerInstanceName()
    {
        return $"Instance_{TestContext.CurrentContext.Test.ID}";
    }

    private static string GetSchedulerInstanceId()
    {
        return $"Instance_{TestContext.CurrentContext.Test.ID}";
    }

    private static string GetConnectionString()
    {
        return $"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")};Foreign Keys=True;";
    }

    private static void UseUseSqliteDatabase(DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        builder.UseSqlite(GetConnectionString());
    }
}