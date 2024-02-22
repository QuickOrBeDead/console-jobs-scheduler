using AutoFixture;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.History;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using ConsoleJobScheduler.Core.Domain.History.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class JobHistoryApplicationServiceFixture
{
    [Test]
    public async Task Should_Insert_JobHistoryEntry()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var expected = new Fixture().Create<JobExecutionHistory>();
        expected.SetException(new InvalidOperationException("test"));
        expected.SetLastSignalTime(DateTime.UtcNow);
        expected.SetCompleted(TimeSpan.FromSeconds(55));

        // Act
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
        var serviceProvider = CreateServiceProvider();
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry);
        
        var runTime = TimeSpan.FromSeconds(99);

        // Act
        await jobHistoryApplicationService.UpdateJobHistoryEntryCompleted(entry.Id, runTime, null);

        // Assert
        Assert.That(entry.RunTime, Is.EqualTo(runTime));
        Assert.That(entry.Completed, Is.True);
        Assert.That(entry.HasError, Is.False);
        Assert.That(entry.ErrorMessage, Is.Null);
        Assert.That(entry.ErrorDetails, Is.Null); 
        
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
        var serviceProvider = CreateServiceProvider();
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry);
        
        var runTime = TimeSpan.FromSeconds(99);
        var exception = new InvalidOperationException("test");

        // Act
        await jobHistoryApplicationService.UpdateJobHistoryEntryCompleted(entry.Id, runTime, exception);

        // Assert
        Assert.That(entry.RunTime, Is.EqualTo(runTime));
        Assert.That(entry.Completed, Is.True);
        Assert.That(entry.HasError, Is.True);
        Assert.That(entry.ErrorMessage, Is.EqualTo(exception.Message));
        Assert.That(entry.ErrorDetails, Is.EqualTo(exception.ToString()));
        
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        var actual = await dbContext.FindAsync<JobExecutionHistory>(entry.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual!.Completed, Is.True);
        Assert.That(actual.RunTime, Is.EqualTo(runTime));
        Assert.That(actual.HasError, Is.True);
        Assert.That(actual.ErrorMessage, Is.EqualTo(exception.Message));
        Assert.That(actual.ErrorDetails, Is.EqualTo(exception.ToString()));

        var errorDetails = await jobHistoryApplicationService.GetJobExecutionErrorDetail(entry.Id);
        Assert.That(errorDetails, Is.EqualTo(exception.ToString()));
    }
    
    [Test]
    public async Task Should_Update_History_Vetoed()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry);
        
        // Act
        await jobHistoryApplicationService.UpdateJobHistoryEntryVetoed(entry.Id);

        // Assert
        Assert.That(entry.Vetoed, Is.True);
        
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
        var serviceProvider = CreateServiceProvider();
        var jobHistoryApplicationService = serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
        
        var entry = new Fixture().Create<JobExecutionHistory>();
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry);
        
        var signalTime = DateTime.UtcNow;

        // Act
        await jobHistoryApplicationService.UpdateJobHistoryEntryLastSignalTime(entry.Id, signalTime);

        // Assert
        Assert.That(entry.LastSignalTime, Is.EqualTo(signalTime));
        
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
        
        var jobHistoryApplicationService = CreateJobHistoryApplicationService(fakeTimeProvider);
        var entry1 = CreateJbJobExecutionHistory("1", "Job1", new DateTime(2024, 1, 1, 10, 11, 12, DateTimeKind.Utc));
        var entry2 = CreateJbJobExecutionHistory("2", "Job2", new DateTime(2024, 1, 1, 10, 10, 10, DateTimeKind.Utc));
        
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry1);
        await jobHistoryApplicationService.InsertJobHistoryEntry(entry2);
        
        // Act
        var page1 = await jobHistoryApplicationService.ListJobExecutionHistory(string.Empty, 1, 1);
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
        var jobHistoryApplicationService = CreateJobHistoryApplicationService();
        var job1Entry = CreateJbJobExecutionHistory("1", "Job1", new DateTime(2024, 1, 1, 10, 11, 12, DateTimeKind.Utc));
        var job2Entry = CreateJbJobExecutionHistory("2", "Job2", new DateTime(2024, 1, 1, 10, 10, 10, DateTimeKind.Utc));
        job2Entry.SetCompleted(TimeSpan.FromSeconds(99));
        
        await jobHistoryApplicationService.InsertJobHistoryEntry(job1Entry);
        await jobHistoryApplicationService.InsertJobHistoryEntry(job2Entry);
        
        // Act
        var actual = await jobHistoryApplicationService.ListJobExecutionHistory("Job2", 1, 1);

        // Assert
        Assert.That(actual.TotalCount, Is.EqualTo(1));
        
        var item = actual.Items.Single();
        Assert.That(item.HasSignalTimeout, Is.EqualTo(false));
        AssertModel(item, job2Entry);
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

    private static JobExecutionHistory CreateJbJobExecutionHistory(string id, string jobName, DateTime lastSignalTime = default, string? cronExpression = null)
    {
        return new JobExecutionHistory(id, "Scheduler1", "Instance1", "ConsolePackage", jobName, "Package", "Trigger1",
            "TriggerGroup1", null, DateTime.Now, lastSignalTime, DateTime.Now, cronExpression);
    }

    private IJobHistoryApplicationService CreateJobHistoryApplicationService(TimeProvider? timeProvider = null)
    {
        var serviceProvider = CreateServiceProvider(timeProvider);
        return serviceProvider.GetRequiredService<IJobHistoryApplicationService>();
    }

    private static ServiceProvider CreateServiceProvider(TimeProvider? timeProvider = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var jobHistoryModule = new JobHistoryModule(Substitute.For<IConfigurationRoot>());
        jobHistoryModule.Register(services, builder => builder.UseInMemoryDatabase($"JobHistoryApplicationServiceTest{TestContext.CurrentContext.Test.ID}"));

        services.AddScoped<IJobHistoryApplicationService, JobHistoryApplicationService>();

        services.AddSingleton(timeProvider ?? new FakeTimeProvider());

        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<HistoryDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return serviceProvider;
    }
}