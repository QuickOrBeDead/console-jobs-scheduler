namespace ConsoleJobScheduler.Service;

using ConsoleJobScheduler.Service.Api.Hubs;
using ConsoleJobScheduler.Service.Api.Hubs.Handlers;
using ConsoleJobScheduler.Service.Api.Models;
using ConsoleJobScheduler.Service.Infrastructure.Scheduler;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.Cookies;
using ConsoleJobScheduler.Service.Infrastructure.Identity;

public sealed class ServiceHost
{
    private bool _stopRequested;
    private WebApplication? _app;

    private readonly string _contentRootPath;
    private readonly ISchedulerServiceBuilder _schedulerServiceBuilder;

    private ISchedulerService? _schedulerService;

    public ServiceHost(IConfiguration config, string contentRootPath)
    {
        _contentRootPath = contentRootPath;
        _schedulerServiceBuilder = new SchedulerServiceBuilder(config ?? throw new ArgumentNullException(nameof(config)));
    }

    public async Task Start()
    {
        _schedulerService = await _schedulerServiceBuilder.Build().ConfigureAwait(false);

        await StartWebHost(_schedulerService).ConfigureAwait(false);
    }

    public async Task Stop()
    {
        _stopRequested = true;

        if (_schedulerService != null)
        {
            await _schedulerService.Shutdown().ConfigureAwait(false);
        }

        if (_app != null)
        {
            await _app.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task StartWebHost(ISchedulerService schedulerService)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = _contentRootPath });
        builder.Services.Configure<HostOptions>(
            option =>
                {
                    option.ShutdownTimeout = TimeSpan.FromSeconds(60);
                });

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSignalR();
        builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

        builder.Services.AddSingleton(schedulerService);

        builder.Services.AddSingleton<JobConsoleLogMessageToHubHandler>();
        builder.Services.AddSpaStaticFiles(configuration: options => { options.RootPath = "wwwroot"; });

        builder.Services.AddDbContext<IdentityManagementDbContext>(o => o.UseNpgsql(builder.Configuration["ConnectionString"]));
        builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>
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

        builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, option =>
            {
                option.LoginPath = new PathString("/api/auth/Login");
                option.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
            });

        _app = builder.Build();
        _app.Lifetime.ApplicationStopped.Register(
            () =>
                {
                    if (!_stopRequested)
                    {
                        Stop().GetAwaiter().GetResult();
                    }
                });

        // Configure the HTTP request pipeline.
        if (!_app.Environment.IsDevelopment())
        {
            _app.UseExceptionHandler("/Error");
        }
        else
        {
            _app.UseSwagger();
            _app.UseSwaggerUI();
        }

        _app.UseWhen(x => 
                x.Request.Path.HasValue && 
                !x.Request.Path.Value.StartsWith("/api", StringComparison.InvariantCultureIgnoreCase) &&
                !x.Request.Path.Value.StartsWith("/jobRunConsoleHub", StringComparison.InvariantCultureIgnoreCase), app1 =>
            app1.UseSpa(spa =>
                {
                    if (_app.Environment.IsDevelopment())
                    {
                        spa.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                    }
                })
        );

        _app.UseSpaStaticFiles();
        _app.UseStaticFiles();
        _app.UseRouting();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.MapHub<JobRunConsoleHub>("/jobRunConsoleHub");

        schedulerService.SubscribeToEvent(_app.Services.GetRequiredService<JobConsoleLogMessageToHubHandler>());

        using var scope = _app.Services.CreateScope();
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<int>>>();
        using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var roles = new[] {Roles.Admin, Roles.JobEditor, Roles.JobViewer};
        for (var i = 0; i < roles.Length; i++)
        {
            var role = roles[i];
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            adminUser = new IdentityUser<int>("admin") {Email = "admin@email.com"};
            
            await userManager.CreateAsync(adminUser, "Password");
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }

        await schedulerService.Start(_app.Services.GetRequiredService<ILoggerFactory>());
        await _app.RunAsync(_app.Configuration["WebAppUrl"]).ConfigureAwait(false);
    }
}