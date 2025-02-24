using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Model;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using ConsoleJobScheduler.Core.Domain.Identity.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace ConsoleJobScheduler.Core.Tests.Application;

[TestFixture]
public sealed class IdentityApplicationServiceFixture
{
    [Test]
    public async Task Admin_Should_Add_Default_Roles_And_Users_When_Application_First_Initialized()
    {
        // Arrange
        var identityApplicationService = await CreateIdentityApplicationService();
        
        // Act
        await identityApplicationService.AddInitialRolesAndUsers();

        // Assert
        var roles = await identityApplicationService.GetAllRoles();
        var users = await identityApplicationService.ListUsers();
        
        Assert.That(roles, Is.EquivalentTo(new[] { Roles.Admin, Roles.JobEditor, Roles.JobViewer }));
        Assert.That(users.Items.Count(), Is.EqualTo(1));
        Assert.That(users.TotalCount, Is.EqualTo(1));
        Assert.That(users.Items.First().Id, Is.GreaterThan(0));
        Assert.That(users.Items.First().UserName, Is.EqualTo(IdentityApplicationService.DefaultAdminUserName));
        Assert.That(users.Items.First().Roles, Is.EqualTo(Roles.Admin));
    }

    [Test]
    public async Task Admin_Should_Login_With_Default_Password_When_Application_First_Initialized()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        
        await arrangeService.AddInitialRolesAndUsers();
        
        // Act
        var identityApplicationService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var signInResult = await identityApplicationService.Login(new LoginModel
        {
            UserName = IdentityApplicationService.DefaultAdminUserName,
            Password = IdentityApplicationService.DefaultAdminPassword
        });

        // Assert
        Assert.That(signInResult.Succeeded, Is.True);
        Assert.That(httpContext.User.Identity, Is.Not.Null);
        Assert.That(httpContext.User.Identity!.IsAuthenticated, Is.True);

        var currentUser = identityApplicationService.GetUserContext(httpContext.User)!;
       
        Assert.That(currentUser, Is.Not.Null);
        Assert.That(currentUser.UserName, Is.EqualTo(IdentityApplicationService.DefaultAdminUserName));
        Assert.That(currentUser.Roles, Is.EquivalentTo(new[] { Roles.Admin }));
    }

    [Test]
    public async Task Should_Add_New_User()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        await arrangeService.AddInitialRolesAndUsers();
        var userAddOrUpdateModel = new UserAddOrUpdateModel
        {
            UserName = "test",
            Password = "12345678",
            Roles = new List<string> { Roles.JobViewer, Roles.JobEditor }
        };
        
        // Act
        var identityApplicationService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var result = await identityApplicationService.SaveUser(userAddOrUpdateModel);

        // Assert
        Assert.That(result.Succeeded, Is.True);
        
        var user = await identityApplicationService.GetUserForEdit(userAddOrUpdateModel.Id);
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Id, Is.EqualTo(userAddOrUpdateModel.Id));
        Assert.That(user.UserName, Is.EqualTo(userAddOrUpdateModel.UserName));
        Assert.That(user.Roles, Is.EquivalentTo(userAddOrUpdateModel.Roles));
    }
    
    [Test]
    public async Task Should_Update_Existing_User()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        await arrangeService.AddInitialRolesAndUsers();
        var userAddOrUpdateModel = new UserAddOrUpdateModel
        {
            UserName = "test",
            Password = "12345678",
            Roles = new List<string> { Roles.JobViewer, Roles.JobEditor }
        };
        await arrangeService.SaveUser(userAddOrUpdateModel);
       
        var userToEdit = await arrangeService.GetUserForEdit(userAddOrUpdateModel.Id);
        
        Assert.That(userToEdit, Is.Not.Null);
        
        var userToUpdate = new UserAddOrUpdateModel
        {
            Id = userToEdit!.Id,
            UserName = userToEdit.UserName,
            Roles = new List<string> { Roles.Admin }
        };
        
        // Act
        var identityApplicationService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var result = await identityApplicationService.SaveUser(userToUpdate);
        
        // Assert
        Assert.That(result.Succeeded, Is.True);
        
        var user = await identityApplicationService.GetUserForEdit(userAddOrUpdateModel.Id);
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Id, Is.EqualTo(userToUpdate.Id));
        Assert.That(user.UserName, Is.EqualTo(userToUpdate.UserName));
        Assert.That(user.Roles, Is.EquivalentTo(userToUpdate.Roles));
    }
    
    [Test]
    public async Task Should_Update_Password_Of_Existing_User()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var arrangeService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        
        await arrangeService.AddInitialRolesAndUsers();
        var userAddOrUpdateModel = new UserAddOrUpdateModel
        {
            UserName = "test",
            Password = "12345678",
            Roles = new List<string> { Roles.JobViewer, Roles.JobEditor }
        };
        await arrangeService.SaveUser(userAddOrUpdateModel);
       
        var userToEdit = await arrangeService.GetUserForEdit(userAddOrUpdateModel.Id);
        
        Assert.That(userToEdit, Is.Not.Null);
        
        var userToUpdate = new UserAddOrUpdateModel
        {
            Id = userToEdit!.Id,
            UserName = userToEdit.UserName,
            Password = "87654321",
            Roles = new List<string> { Roles.Admin }
        };
        
        // Act
        var identityApplicationService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var result = await identityApplicationService.SaveUser(userToUpdate);
        
        // Assert
        Assert.That(result.Succeeded, Is.True);
        
        var user = await identityApplicationService.GetUserForEdit(userAddOrUpdateModel.Id);
        
        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Id, Is.EqualTo(userToUpdate.Id));
        Assert.That(user.UserName, Is.EqualTo(userToUpdate.UserName));
        Assert.That(user.Roles, Is.EquivalentTo(userToUpdate.Roles));

        var loginResult = await identityApplicationService.Login(new LoginModel { UserName = userToUpdate.UserName, Password = userToUpdate.Password });
        
        Assert.That(loginResult.Succeeded, Is.True);

        var userContext = identityApplicationService.GetUserContext(httpContext.User);
        
        Assert.That(userContext, Is.Not.Null);
        Assert.That(userContext!.UserName, Is.EquivalentTo(userToUpdate.UserName));
        Assert.That(userContext.Roles, Is.EquivalentTo(userToUpdate.Roles));
    }

    [Test]
    public async Task User_Context_Should_Be_Null_When_User_Is_Not_LoggedIn()
    {
        // Arrange
        var serviceProvider = await CreateServiceProvider();
        var identityApplicationService = serviceProvider.GetRequiredService<IIdentityApplicationService>();
        var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        
        await identityApplicationService.AddInitialRolesAndUsers();
       
        // Act
        var result = identityApplicationService.GetUserContext(httpContext.User);
        
        // Assert
        Assert.That(result, Is.Null);
    }
    
    private static async Task<IIdentityApplicationService> CreateIdentityApplicationService()
    {
        var serviceProvider = await CreateServiceProvider();
        return serviceProvider.GetRequiredService<IIdentityApplicationService>();
    }
    
    private static async Task<IServiceProvider> CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication();
        
        services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, option =>
        {
            option.LoginPath = new PathString("/api/auth/Login");
            option.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });

        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        services.AddSingleton(_ => httpContextAccessor);
        
        var identityModule = new IdentityModule(Substitute.For<IConfigurationRoot>());
        identityModule.Register(services, builder =>
        {
            builder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            builder.UseSqlite($"DataSource={Path.Combine(TestContext.CurrentContext.TestDirectory, "test.db")}");
        });
        
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<IdentityManagementDbContext>();
        await dbContext.Database.EnsureDeletedAsync();

        await identityModule.MigrateDb(serviceProvider);
        
        ConfigureHttpContext(serviceProvider, httpContextAccessor);
        return serviceProvider;
    }

    private static void ConfigureHttpContext(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        serviceScopeFactory.CreateScope().Returns(_ => serviceProvider.CreateScope());
        
        var featureCollection = new FeatureCollection();
        featureCollection.Set<IHttpResponseFeature>(new HttpResponseFeature());
        featureCollection.Set<IHttpRequestFeature>(new HttpRequestFeature());
        featureCollection.Set<IRequestCookiesFeature>(new RequestCookiesFeature(featureCollection));
        featureCollection.Set<IAuthenticationFeature>(new AuthenticationFeature());
        
        var httpContext = new DefaultHttpContext(featureCollection);
        httpContextAccessor.HttpContext.Returns(httpContext);
        featureCollection.Set<IServiceProvidersFeature>(new RequestServicesFeature(httpContext, serviceScopeFactory));
    }
}