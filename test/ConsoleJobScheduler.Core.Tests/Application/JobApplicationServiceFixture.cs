using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.Runner.Infra;
using ConsoleJobScheduler.Core.Infra.EMail;
using ConsoleJobScheduler.Core.Infra.EMail.Model;
using ConsoleJobScheduler.Messaging.Models;
using MessagePipe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Z.EntityFramework.Extensions;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class JobApplicationServiceFixture
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
    public async Task Should_Send_Email_Message()
    {
        // Arrange
        var jobRunId = "1";

        var serviceProvider = await CreateServiceProvider();
        var jobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();

        var emailMessage = new EmailMessage
        {
            To = "to@email.com",
            CC = "cc@emai.com",
            Bcc = "bcc@email.com",
            Subject = "Subject",
            Body = "body"
        };

        emailMessage.AddAttachment("test.txt", "text/plain", "test"u8.ToArray());

        // Act
        var emailId = await jobApplicationService.SendEmailMessage(jobRunId, emailMessage);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        var email = await dbContext.JobRunEmails.Include(x => x.Attachments).Where(x => x.Id == emailId)
            .SingleOrDefaultAsync();

        var jobExecutionDetail = await serviceProvider.GetRequiredService<IJobApplicationService>()
            .GetJobExecutionDetail(jobRunId);

        Assert.That(email, Is.Not.Null);
        Assert.That(email!.To, Is.EqualTo(emailMessage.To));
        Assert.That(email.CC, Is.EqualTo(emailMessage.CC));
        Assert.That(email.Bcc, Is.EqualTo(emailMessage.Bcc));
        Assert.That(email.Subject, Is.EqualTo(emailMessage.Subject));
        Assert.That(email.Body, Is.EqualTo(emailMessage.Body));
        Assert.That(email.CreateDate, Is.GreaterThan(default(DateTime)));
        Assert.That(email.IsSent, Is.True);

        Assert.That(email.Attachments.Count, Is.EqualTo(1));
        Assert.That(email.Attachments[0].ContentType, Is.EqualTo(emailMessage.Attachments[0].ContentType));
        Assert.That(email.Attachments[0].EmailId, Is.EqualTo(emailId));
        Assert.That(email.Attachments[0].CreateDate, Is.GreaterThan(default(DateTime)));
        Assert.That(email.Attachments[0].Content, Is.EquivalentTo(emailMessage.Attachments[0].GetContentBytes()));
        Assert.That(email.Attachments[0].FileName, Is.EqualTo(emailMessage.Attachments[0].FileName));
        Assert.That(email.Attachments[0].JobRunId, Is.EqualTo(jobRunId));

        Assert.That(jobExecutionDetail, Is.Not.Null);
        Assert.That(jobExecutionDetail!.Attachments.Count, Is.EqualTo(1));
        Assert.That(jobExecutionDetail.Attachments[0].FileName, Is.EqualTo(emailMessage.Attachments[0].FileName));
        Assert.That(jobExecutionDetail.Attachments[0].Id, Is.GreaterThan(0));
        
        Assert.That(jobExecutionDetail.Logs.Count, Is.EqualTo(2));
        Assert.That(jobExecutionDetail.Logs[0].Content, Is.EqualTo("Sending email to to@email.com"));
        Assert.That(jobExecutionDetail.Logs[0].IsError, Is.EqualTo(false));
        Assert.That(jobExecutionDetail.Logs[0].CreateDate, Is.GreaterThan(default(DateTime)));
        Assert.That(jobExecutionDetail.Logs[1].Content, Is.EqualTo("Email is sent to to@email.com"));
        Assert.That(jobExecutionDetail.Logs[1].IsError, Is.EqualTo(false));
        Assert.That(jobExecutionDetail.Logs[1].CreateDate, Is.GreaterThan(default(DateTime)));
        Assert.That(jobExecutionDetail.Logs[1].CreateDate, Is.GreaterThan(jobExecutionDetail.Logs[0].CreateDate));
        
        var attachmentContent = await serviceProvider.GetRequiredService<IJobApplicationService>()
            .GetJobRunAttachmentContent(email.Attachments[0].Id);
        Assert.That(attachmentContent, Is.EquivalentTo(email.Attachments[0].Content));
    }

    [Test]
    public async Task Should_Insert_Job_Run_Attachment()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var jobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();
        var attachment = AttachmentModel.Create("1", "test.txt", "test"u8.ToArray(), "text/plain");

        // Act
        var attachmentId = await jobApplicationService.InsertJobRunAttachment(attachment);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        var actualAttachment = await dbContext.JobRunAttachments.FindAsync(attachmentId);
        var attachmentContent = await serviceProvider.GetRequiredService<IJobApplicationService>()
            .GetJobRunAttachmentContent(attachmentId);
        
        Assert.That(attachmentContent, Is.EquivalentTo(attachment.FileContent));
        Assert.That(actualAttachment!.ContentType, Is.EqualTo(attachment.ContentType));
        Assert.That(actualAttachment.EmailId, Is.Null);
        Assert.That(actualAttachment.CreateDate, Is.GreaterThan(default(DateTime)));
        Assert.That(actualAttachment.Content, Is.EquivalentTo(attachment.FileContent));
        Assert.That(actualAttachment.FileName, Is.EqualTo(attachment.FileName));
        Assert.That(actualAttachment.JobRunId, Is.EqualTo(attachment.JobRunId));
    }

    [Test]
    public async Task Should_Add_Package()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var jobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();
        
        // Act
        await jobApplicationService.SavePackage("GithubReadmeStats", await ReadPackageManifest("GithubReadmeStats.zip"));
        await jobApplicationService.SavePackage("NbaMatches", await ReadPackageManifest("NbaMatches.zip"));

        // Assert
        var assertJobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();
        var packageNames = await assertJobApplicationService.GetAllPackageNames();
        var pageList = await assertJobApplicationService.ListPackages();
        var githubReadmeStatsPackage = await assertJobApplicationService.GetPackageDetails("GithubReadmeStats");
        var nbaMatchesPackage = await assertJobApplicationService.GetPackageDetails("NbaMatches");
        
        Assert.That(packageNames, Is.EquivalentTo(new [] { "GithubReadmeStats", "NbaMatches" }));
        Assert.That(pageList.TotalCount, Is.EqualTo(2));
        Assert.That(pageList.Items.ToList()[0].Name, Is.EqualTo("GithubReadmeStats"));
        Assert.That(pageList.Items.ToList()[1].Name, Is.EqualTo("NbaMatches"));
        Assert.That(githubReadmeStatsPackage, Is.Not.Null);
        Assert.That(githubReadmeStatsPackage!.Name, Is.EqualTo("GithubReadmeStats"));
        Assert.That(githubReadmeStatsPackage.ModifyDate, Is.Not.Null);
        Assert.That(nbaMatchesPackage, Is.Not.Null);
        Assert.That(nbaMatchesPackage!.Name, Is.EqualTo("NbaMatches"));
        Assert.That(nbaMatchesPackage.ModifyDate, Is.Not.Null);
    }

    [Test]
    public async Task Should_Insert_Job_Run_Log()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var jobApplicationService = serviceProvider.GetRequiredService<IJobApplicationService>();
        
        // Act
        var id = await jobApplicationService.InsertJobRunLog("1", "Error", true);

        // Assert
        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        var log = await dbContext.JobRunLogs.FindAsync(id);
        
        Assert.That(log, Is.Not.Null);
        Assert.That(log!.JobRunId, Is.EqualTo("1"));
        Assert.That(log.Content, Is.EqualTo("##[error] Error"));
        Assert.That(log.IsError, Is.True);
        Assert.That(log.CreateDate, Is.GreaterThan(default(DateTime)));
    }

    private static Task<byte[]> ReadPackageManifest(string manifestJsonFile)
    {
        return File.ReadAllBytesAsync(Path.Combine(TestContext.CurrentContext.TestDirectory, "Application", "_Data", manifestJsonFile));
    }

    private static async Task<ServiceProvider> CreateServiceProvider()
    {
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
        
        var jobHistoryModule = new JobRunModule(Substitute.For<IConfigurationRoot>());
        jobHistoryModule.Register(services, UseUseSqliteDatabase);
        
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<RunnerDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        // await jobHistoryModule.MigrateDb(serviceProvider);
        
        return serviceProvider;
    }

    private static void UseUseSqliteDatabase(DbContextOptionsBuilder builder)
    {
        builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        builder.UseSqlite($"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")}");
    }
}