using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core;

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
    public void Test()
    {
        // Arrange
        var migrationRunner = new DbMigrationRunner();

        // Act
        migrationRunner.Migrate(_postgresOptions.GetConnectionString(), "qrtz_", version: 1);

        // Assert
    }
}