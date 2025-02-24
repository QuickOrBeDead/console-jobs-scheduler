using AutoFixture;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Z.EntityFramework.Extensions;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class JobHistoryApplicationServiceFixture
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
    public async Task Should_Insert_JobHistoryEntry()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();

        var expected = new Fixture().Create<JobExecutionHistory>();
        expected.SetException(new InvalidOperationException("test"));
        expected.SetLastSignalTime(DateTime.UtcNow);
        expected.SetCompleted(TimeSpan.FromSeconds(55));

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(expected);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(expected.Id);
        
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Id, Is.EqualTo(expected.Id));
        Assert.That(actual.FiredTime, Is.EqualTo(expected.FiredTime));
        Assert.That(actual.Completed, Is.EqualTo(expected.Completed));
        Assert.That(actual.Vetoed, Is.EqualTo(expected.Vetoed));
        Assert.That(actual.ErrorDetails, Is.EqualTo(expected.ErrorDetails));
        Assert.That(actual.ErrorMessage, Is.EqualTo(expected.ErrorMessage));
        Assert.That(actual.HasError, Is.EqualTo(expected.HasError));
        Assert.That(actual.InstanceName, Is.EqualTo(expected.InstanceName));
        Assert.That(actual.JobGroup, Is.EqualTo(expected.JobGroup));
        Assert.That(actual.JobName, Is.EqualTo(expected.JobName));
        Assert.That(actual.PackageName, Is.EqualTo(expected.PackageName));
        Assert.That(actual.RunTime, Is.EqualTo(expected.RunTime));
        Assert.That(actual.ScheduledTime, Is.EqualTo(expected.ScheduledTime));
        Assert.That(actual.SchedulerName, Is.EqualTo(expected.SchedulerName));
        Assert.That(actual.TriggerGroup, Is.EqualTo(expected.TriggerGroup));
        Assert.That(actual.TriggerName, Is.EqualTo(expected.TriggerName));
        Assert.That(actual.CronExpressionString, Is.EqualTo(expected.CronExpressionString));
        Assert.That(actual.LastSignalTime, Is.EqualTo(expected.LastSignalTime));
        Assert.That(actual.NextFireTime, Is.EqualTo(expected.NextFireTime));
    }
    
    [Test]
    public async Task Should_Update_History_Completed_Without_Exception()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await arrangeService.InsertJobHistoryEntry(entry);
        
        var runTime = TimeSpan.FromSeconds(99);

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.UpdateJobHistoryEntryCompleted(entry.Id, runTime, null);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(entry.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Completed, Is.True);
        Assert.That(actual.RunTime, Is.EqualTo(runTime));
        Assert.That(actual.HasError, Is.False);
        Assert.That(actual.ErrorMessage, Is.Null);
        Assert.That(actual.ErrorDetails, Is.Null); 
    }
    
    [Test]
    public async Task Should_Update_History_Completed_With_Exception()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await arrangeService.InsertJobHistoryEntry(entry);
        
        var runTime = TimeSpan.FromSeconds(99);
        var exception = new InvalidOperationException("test");

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.UpdateJobHistoryEntryCompleted(entry.Id, runTime, exception);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(entry.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Completed, Is.True);
        Assert.That(actual.RunTime, Is.EqualTo(runTime));
        Assert.That(actual.HasError, Is.True);
        Assert.That(actual.ErrorMessage, Is.EqualTo(exception.Message));
        Assert.That(actual.ErrorDetails, Is.EqualTo(exception.ToString()));

        var errorDetails = await arrangeService.GetJobExecutionErrorDetail(entry.Id);
        Assert.That(errorDetails, Is.EqualTo(exception.ToString()));
    }
    
    [Test]
    public async Task Should_Update_History_Vetoed()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await arrangeService.InsertJobHistoryEntry(entry);
        
        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.UpdateJobHistoryEntryVetoed(entry.Id);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(entry.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Vetoed, Is.True);
    }
    
    [Test]
    public async Task Should_Update_History_LastSignalTime()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await arrangeService.InsertJobHistoryEntry(entry);
        
        var signalTime = DateTime.UtcNow;

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await jobHistoryApplicationService.UpdateJobHistoryEntryLastSignalTime(entry.Id, signalTime);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(entry.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.LastSignalTime, Is.EqualTo(signalTime));
    }

    [Test]
    public async Task Should_List_JobExecutionHistory()
    {
        // Arrange
        var now = new DateTime(2024, 1, 1, 10, 12, 0, DateTimeKind.Utc);
        var fakeTimeProvider = new FakeTimeProvider(now);

        var serviceProvider = await CreateServiceProvider(fakeTimeProvider);
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var entry1 = CreateJobExecutionHistory("1", "Job1", new DateTime(2024, 1, 1, 10, 11, 12, DateTimeKind.Utc));
        var entry2 = CreateJobExecutionHistory("2", "Job2", new DateTime(2024, 1, 1, 10, 10, 10, DateTimeKind.Utc));
        
        await arrangeService.InsertJobHistoryEntry(entry1);
        await arrangeService.InsertJobHistoryEntry(entry2);
        
        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var page1 = await jobHistoryApplicationService.ListJobExecutionHistory(string.Empty, 1);
        var page2 = await jobHistoryApplicationService.ListJobExecutionHistory(string.Empty, 1, 2);

        // Assert
        Assert.That(page1.TotalCount, Is.EqualTo(2));
        
        var actual1 = page1.Items.Single();
        Assert.That(actual1.HasSignalTimeout, Is.EqualTo(true));
        AssertModel(actual1, entry2);

        Assert.That(page2.TotalCount, Is.EqualTo(2));
        
        var actual2 = page2.Items.Single();
        Assert.That(actual2.HasSignalTimeout, Is.EqualTo(false));
        AssertModel(actual2, entry1);
    }
    
    [Test]
    public async Task Should_List_JobExecutionHistory_By_JobName()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var job1Entry = CreateJobExecutionHistory("1", "Job1", new DateTime(2024, 1, 1, 10, 11, 12, DateTimeKind.Utc));
        var job2Entry = CreateJobExecutionHistory("2", "Job2", new DateTime(2024, 1, 1, 10, 10, 10, DateTimeKind.Utc));
        job2Entry.SetCompleted(TimeSpan.FromSeconds(99));
        
        await arrangeService.InsertJobHistoryEntry(job1Entry);
        await arrangeService.InsertJobHistoryEntry(job2Entry);
        
        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var actual = await jobHistoryApplicationService.ListJobExecutionHistory("Job2", 1);

        // Assert
        Assert.That(actual.TotalCount, Is.EqualTo(1));
        
        var item = actual.Items.Single();
        Assert.That(item.HasSignalTimeout, Is.EqualTo(false));
        AssertModel(item, job2Entry);
    }

    [Test]
    public async Task Should_Get_Job_Execution_Statistics()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var fixture = new Fixture();
        var entry1 = fixture.Create<JobExecutionHistory>();
        var entry2 = fixture.Create<JobExecutionHistory>();
        var entry3 = fixture.Create<JobExecutionHistory>();
        var entry4 = fixture.Create<JobExecutionHistory>();
        var entry5 = fixture.Create<JobExecutionHistory>();
        var entry6 = fixture.Create<JobExecutionHistory>();

        await arrangeService.InsertJobHistoryEntry(entry1);
        await arrangeService.InsertJobHistoryEntry(entry2);
        await arrangeService.InsertJobHistoryEntry(entry3);
        await arrangeService.InsertJobHistoryEntry(entry4);
        await arrangeService.InsertJobHistoryEntry(entry5);
        await arrangeService.InsertJobHistoryEntry(entry6);

        await arrangeService.UpdateJobHistoryEntryCompleted(entry1.Id, TimeSpan.FromSeconds(99), null);
        await arrangeService.UpdateJobHistoryEntryCompleted(entry2.Id, TimeSpan.FromSeconds(101), null);
        await arrangeService.UpdateJobHistoryEntryCompleted(entry3.Id, TimeSpan.FromSeconds(77), null);
        await arrangeService.UpdateJobHistoryEntryCompleted(entry4.Id, TimeSpan.FromSeconds(50), new InvalidOperationException("test"));
        await arrangeService.UpdateJobHistoryEntryCompleted(entry5.Id, TimeSpan.FromSeconds(10), new InvalidOperationException("test"));
        await arrangeService.UpdateJobHistoryEntryVetoed(entry6.Id);

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var statistics = await jobHistoryApplicationService.GetJobExecutionStatistics();

        // Assert
        Assert.That(statistics.TotalExecutedJobs, Is.EqualTo(5));
        Assert.That(statistics.TotalVetoedJobs, Is.EqualTo(1));
        Assert.That(statistics.TotalSucceededJobs, Is.EqualTo(3));
        Assert.That(statistics.TotalFailedJobs, Is.EqualTo(2));
    }

    [Test]
    public async Task Should_List_JobExecutionHistoryChartData()
    {
        // Arrange
        var now = new DateTime(2024, 1, 1, 10, 12, 0, DateTimeKind.Utc);
        var fakeTimeProvider = new FakeTimeProvider(now);

        var serviceProvider = await CreateServiceProvider(fakeTimeProvider);
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("1", "Job1", firedTime: new DateTime(2023, 12, 12)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("2", "Job2", firedTime: new DateTime(2024, 1, 1, 9, 0, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("3", "Job3", firedTime: new DateTime(2024, 1, 1, 9, 5, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("4", "Job4", firedTime: new DateTime(2024, 1, 1, 9, 14, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("5", "Job5", firedTime: new DateTime(2024, 1, 1, 9, 16, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("6", "Job6", firedTime: new DateTime(2024, 1, 1, 9, 29, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("7", "Job6", firedTime: new DateTime(2024, 1, 1, 10, 30, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("8", "Job6", firedTime: new DateTime(2024, 1, 1, 10, 59, 0)));
        await arrangeService.InsertJobHistoryEntry(CreateJobExecutionHistory("9", "Job6", firedTime: new DateTime(2024, 1, 1, 12, 31, 0)));

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var chartData = await jobHistoryApplicationService.ListJobExecutionHistoryChartData();

        // Assert
        Assert.That(chartData[0].X, Is.EqualTo(new DateTime(2024, 1, 1, 9, 0, 0)));
        Assert.That(chartData[0].Y, Is.EqualTo(3));
        Assert.That(chartData[1].X, Is.EqualTo(new DateTime(2024, 1, 1, 9, 15, 0)));
        Assert.That(chartData[1].Y, Is.EqualTo(2));
        Assert.That(chartData[2].X, Is.EqualTo(new DateTime(2024, 1, 1, 10, 30, 0)));
        Assert.That(chartData[2].Y, Is.EqualTo(1));
        Assert.That(chartData[3].X, Is.EqualTo(new DateTime(2024, 1, 1, 10, 45, 0)));
        Assert.That(chartData[3].Y, Is.EqualTo(1));
        Assert.That(chartData[4].X, Is.EqualTo(new DateTime(2024, 1, 1, 12, 30, 0)));
        Assert.That(chartData[4].Y, Is.EqualTo(1));
    }

    [Test]
    public async Task Should_Get_JobExecutionDetail()
    {
        // Arrange
        var id = "1";
        var runTime = TimeSpan.FromSeconds(99);

        var serviceProvider = await CreateServiceProvider();
        
        var arrangeService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var entry = CreateJobExecutionHistory(id, "Job1", DateTime.UtcNow, "0 0/5 * 1/1 * ? *", DateTime.UtcNow);
        await arrangeService.InsertJobHistoryEntry(entry);
        await arrangeService.UpdateJobHistoryEntryCompleted(entry.Id, runTime, null);

        // Act
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        var actual = await jobHistoryApplicationService.GetJobExecutionDetail(entry.Id);

        // Assert
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Id, Is.EqualTo(entry.Id));
        Assert.That(actual.InstanceName, Is.EqualTo(entry.InstanceName));
        Assert.That(actual.PackageName, Is.EqualTo(entry.PackageName));
        Assert.That(actual.TriggerGroup, Is.EqualTo(entry.TriggerGroup));
        Assert.That(actual.TriggerName, Is.EqualTo(entry.TriggerName));
        Assert.That(actual.JobName, Is.EqualTo(entry.JobName));
        Assert.That(actual.JobGroup, Is.EqualTo(entry.JobGroup));
        Assert.That(actual.ScheduledTime, Is.EqualTo(entry.ScheduledTime));
        Assert.That(actual.FiredTime, Is.EqualTo(entry.FiredTime));
        Assert.That(actual.RunTime, Is.EqualTo(runTime));
        Assert.That(actual.Completed, Is.EqualTo(true));
        Assert.That(actual.Vetoed, Is.EqualTo(false));
        Assert.That(actual.HasError, Is.EqualTo(false));
        Assert.That(actual.LastSignalTime, Is.EqualTo(entry.LastSignalTime));
        Assert.That(actual.ErrorMessage, Is.Null);
        Assert.That(actual.Id, Is.EqualTo(entry.Id));
        Assert.That(actual.CronExpressionDescription, Is.EqualTo("Every 5 minutes"));
    }
    
    private static void AssertModel(JobExecutionHistoryListItem actual, JobExecutionHistory expected)
    {
        Assert.That(actual.Id, Is.EqualTo(expected.Id));
        Assert.That(actual.FiredTime, Is.EqualTo(expected.FiredTime));
        Assert.That(actual.Completed, Is.EqualTo(expected.Completed));
        Assert.That(actual.Vetoed, Is.EqualTo(expected.Vetoed));
        Assert.That(actual.HasError, Is.EqualTo(expected.HasError));
        Assert.That(actual.JobGroup, Is.EqualTo(expected.JobGroup));
        Assert.That(actual.JobName, Is.EqualTo(expected.JobName));
        Assert.That(actual.RunTime, Is.EqualTo(expected.RunTime));
        Assert.That(actual.ScheduledTime, Is.EqualTo(expected.ScheduledTime));
        Assert.That(actual.TriggerGroup, Is.EqualTo(expected.TriggerGroup));
        Assert.That(actual.TriggerName, Is.EqualTo(expected.TriggerName));
        Assert.That(actual.LastSignalTime, Is.EqualTo(expected.LastSignalTime));
        Assert.That(actual.NextFireTime, Is.EqualTo(expected.NextFireTime));
    }

    private static JobExecutionHistory CreateJobExecutionHistory(string id, string jobName, DateTime lastSignalTime = default, string? cronExpression = null, DateTime firedTime = default)
    {
        return new JobExecutionHistory(id, "Scheduler1", "Instance1", "ConsolePackage", jobName, "Package", "Trigger1",
            "TriggerGroup1", null, firedTime, lastSignalTime, DateTime.Now, cronExpression);
    }

    private static async Task<ServiceProvider> CreateServiceProvider(TimeProvider? timeProvider = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var jobHistoryModule = new JobHistoryModule(Substitute.For<IConfigurationRoot>());
        jobHistoryModule.Register(services, UseUseSqliteDatabase);
        
        services.AddSingleton(timeProvider ?? new FakeTimeProvider());

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        await jobHistoryModule.MigrateDb(serviceProvider);
        
        return serviceProvider;
    }

    private static void UseUseSqliteDatabase(DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        builder.UseSqlite($"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")}");
    }
}