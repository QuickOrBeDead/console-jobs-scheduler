using ConsoleJobScheduler.Core.Domain.Identity.Infra;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleJobScheduler.Core.Domain.Identity;

public sealed class IdentityModule
{
    private readonly IConfigurationRoot _configuration;

    public IdentityModule(IConfigurationRoot configuration)
    {
        _configuration = configuration;
    }

    public void Register(IServiceCollection services)
    {
        services.AddDbContext<IdentityManagementDbContext>(o => o.UseNpgsql(_configuration["ConnectionString"]));
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
    }
}