using AutoFixture;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Domain.Settings;
using ConsoleJobScheduler.Core.Domain.Settings.Infra;
using ConsoleJobScheduler.Core.Domain.Settings.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Z.EntityFramework.Extensions;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class SettingsApplicationServiceFixture
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        EntityFrameworkManager.ContextFactory = _ =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<SettingsDbContext>();
            UseInMemoryDatabase(optionsBuilder);
            return new SettingsDbContext(optionsBuilder.Options);
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        EntityFrameworkManager.ContextFactory = null;
    }
    
    [Test]
    public async Task Should_Get_Settings_With_Default_Values_When_No_Setting_Records_In_Db()
    {
        await Should_Get_Settings_With_Default_Values_When_No_Setting_Records_In_Db<GeneralSettings>(
            (actual, expected) =>
            {
                Assert.That(actual.PageSize, Is.EqualTo(expected.PageSize));
            });
        
        await Should_Get_Settings_With_Default_Values_When_No_Setting_Records_In_Db<SmtpSettings>(
            (actual, expected) =>
            {
                Assert.That(actual.Domain, Is.EqualTo(expected.Domain));
                Assert.That(actual.UserName, Is.EqualTo(expected.UserName));
                Assert.That(actual.Password, Is.EqualTo(expected.Password));
                Assert.That(actual.From, Is.EqualTo(expected.From));
                Assert.That(actual.Host, Is.EqualTo(expected.Host));
                Assert.That(actual.Port, Is.EqualTo(expected.Port));
                Assert.That(actual.EnableSsl, Is.EqualTo(expected.EnableSsl));
                Assert.That(actual.FromName, Is.EqualTo(expected.FromName));
            });
    }

    [Test]
    public async Task Should_Save_Settings()
    {
        await Should_Save_Settings<GeneralSettings>(
            (actual, expected) =>
            {
                Assert.That(actual.PageSize, Is.EqualTo(expected.PageSize));
            });
        
        await Should_Save_Settings<SmtpSettings>(
            (actual, expected) =>
            {
                Assert.That(actual.Domain, Is.EqualTo(expected.Domain));
                Assert.That(actual.UserName, Is.EqualTo(expected.UserName));
                Assert.That(actual.Password, Is.EqualTo(expected.Password));
                Assert.That(actual.From, Is.EqualTo(expected.From));
                Assert.That(actual.Host, Is.EqualTo(expected.Host));
                Assert.That(actual.Port, Is.EqualTo(expected.Port));
                Assert.That(actual.EnableSsl, Is.EqualTo(expected.EnableSsl));
                Assert.That(actual.FromName, Is.EqualTo(expected.FromName));
            });
    }
    
    private async Task Should_Save_Settings<TSettings>(Action<TSettings, TSettings> assertAction)
        where TSettings : ISettings, new()
    {
        // Arrange
        var expected = new Fixture().Create<TSettings>();
        
        var settingsApplicationService = CreateSettingsApplicationService();

        // Act
        await settingsApplicationService.SaveSettings(expected);
        
        var actual = await settingsApplicationService.GetSettings<TSettings>();

        // Assert
        Assert.That(actual.GetCategory(), Is.EqualTo(expected.GetCategory()));

        assertAction(actual, expected);
    }
    
    private async Task Should_Get_Settings_With_Default_Values_When_No_Setting_Records_In_Db<TSettings>(Action<TSettings, TSettings> assertAction)
        where TSettings : ISettings, new()
    {
        // Arrange
        var expected = new TSettings();
        expected.Map(new SettingsData());
        
        var settingsApplicationService = CreateSettingsApplicationService();

        // Act
        var actual = await settingsApplicationService.GetSettings<TSettings>();

        // Assert
        Assert.That(actual.GetCategory(), Is.EqualTo(expected.GetCategory()));

        assertAction(actual, expected);
    }
    
    private ISettingsApplicationService CreateSettingsApplicationService()
    {
        IServiceCollection services = new ServiceCollection();
        var settingsModule = new SettingsModule(Substitute.For<IConfigurationRoot>());
        settingsModule.Register(services, UseInMemoryDatabase);

        services.AddScoped<ISettingsApplicationService, SettingsApplicationService>();

        return services.BuildServiceProvider().GetRequiredService<ISettingsApplicationService>();
    }

    private static void UseInMemoryDatabase(DbContextOptionsBuilder builder)
    {
        builder.UseInMemoryDatabase($"{nameof(SettingsApplicationServiceFixture)}{TestContext.CurrentContext.Test.ID}");
    }
}