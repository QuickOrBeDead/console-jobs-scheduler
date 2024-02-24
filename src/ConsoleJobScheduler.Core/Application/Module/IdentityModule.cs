using ConsoleJobScheduler.Core.Domain.Identity;
using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Application.Module;

public sealed class IdentityModule
{
    private readonly IConfigurationRoot _configuration;

    public IdentityModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services, Action<DbContextOptionsBuilder>? dbContextOptionsBuilderAction = null)
    {
        services.AddDbContext<IdentityManagementDbContext>(o =>
        {
            if (dbContextOptionsBuilderAction == null)
            {
                o.UseNpgsql(_configuration["ConnectionString"]);
            }
            else
            {
                dbContextOptionsBuilderAction(o);
            }
        });
        services.AddIdentity<IdentityUser<int>, IdentityRole<int>>
            (options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<IdentityManagementDbContext>()
            .AddDefaultTokenProviders();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IIdentityApplicationService, IdentityApplicationService>();
    }

    public async Task MigrateDb(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        await using var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityManagementDbContext>();
        await identityDbContext.Database.MigrateAsync().ConfigureAwait(false);
    }
}