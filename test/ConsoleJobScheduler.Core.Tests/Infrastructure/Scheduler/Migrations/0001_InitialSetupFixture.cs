using ConsoleJobScheduler.Core.Infra.Migration;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations;
using Quartz;

namespace ConsoleJobScheduler.Core.Tests.Infrastructure.Scheduler.Migrations;

[Category("DbMigration")]
[TestFixture]
public class InitialSetupFixture 
{
    private PostgresDatabase.PostgresOptions _postgresOptions = new PostgresDatabase.PostgresOptions
    {
        Host = "localhost", 
        Port = 5432, 
        User = "quartz", 
        Password = "quartz", 
        Db = "quartz_test"
    };

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        PostgresDatabase.Create(_postgresOptions);
    }
    
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // PostgresDatabase.Delete(_postgresOptions);
    }

    [Test]
    public async Task Test()
    {
        // Arrange
        var migrationRunner = new DbMigrationRunner();

        // Act
        migrationRunner.Migrate(_postgresOptions.GetConnectionString(), "qrtz_", "Scheduler", version: 1);
        
        // Assert
        var schedulerBuilder = SchedulerBuilder.Create()
            .WithId("SchedulerInstanceId")
            .WithName("ConsoleJobsSchedulerService")
            .UsePersistentStore(
                o =>
                {
                    o.UseClustering();
                    o.UseProperties = true;
                    o.UseNewtonsoftJsonSerializer();
                    o.UsePostgres(
                        p =>
                        {
                            p.TablePrefix = "qrtz_";
                            p.ConnectionString = _postgresOptions.GetConnectionString();
                        });
                });
        var scheduler = await schedulerBuilder.BuildScheduler();
        var job = JobBuilder.Create<TestJob>()
            .WithIdentity("job1", "group1")
            .Build();
        
        var trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(100)
                .WithRepeatCount(1))
            .Build();
        
        await scheduler.ScheduleJob(job, trigger);
        await scheduler.Start();
        await scheduler.Shutdown();
    }
    
    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}