using ConsoleJobScheduler.Core.Api.Hubs;
using ConsoleJobScheduler.Core.Api.Hubs.Handlers;
using ConsoleJobScheduler.Core.Api.Models;
using ConsoleJobScheduler.Core.Infrastructure.Identity;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Jobs.Events;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Plugins;
using ConsoleJobScheduler.Core.Infrastructure.Settings.Data;
using ConsoleJobScheduler.Core.Infrastructure.Settings.Service;

using MessagePipe;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Quartz.Util;

namespace ConsoleJobScheduler.Core;

public sealed class ServiceHost
{
    private bool _stopRequested;
    private WebApplication? _app;

    private ISchedulerManager? _schedulerManager;

    public async Task Start()
    {
        await StartWebHost().ConfigureAwait(false);
    }

    private async Task Stop()
    {
        _stopRequested = true;

        if (_schedulerManager != null)
        {
            await _schedulerManager.Shutdown().ConfigureAwait(false);
        }

        if (_app != null)
        {
            await _app.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task StartWebHost()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.Configure<HostOptions>(option => option.ShutdownTimeout = TimeSpan.FromSeconds(60));

        // Add services to the container.
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSignalR();
        builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

        builder.Services.AddSingleton<JobConsoleLogMessageToHubHandler>();

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

        builder.Services.AddSingleton<ISchedulerFactory>(x =>
        {
            var schedulerBuilder = SchedulerBuilder.Create()
                .WithId(builder.Configuration["SchedulerInstanceId"]!)
                .WithName("ConsoleJobsSchedulerService")
                .UseDefaultThreadPool(y => y.MaxConcurrency = 100)
                .UseJobFactory<ServiceProviderJobFactory>()
                .UsePersistentStore(
                    o =>
                    {
                        o.UseClustering();
                        o.UseProperties = true;
                        o.UseNewtonsoftJsonSerializer();
                        o.UsePostgres(
                                p =>
                                {
                                    p.TablePrefix = builder.Configuration["TablePrefix"]!;
                                    p.ConnectionString = builder.Configuration["ConnectionString"]!;
                                });
                    });
            schedulerBuilder.SetProperty(StdSchedulerFactory.PropertyJobStoreType, typeof(CustomJobStoreTx).AssemblyQualifiedNameWithoutVersion());
            schedulerBuilder.SetProperty(JobExecutionHistoryPlugin.PluginConfigurationProperty, typeof(JobExecutionHistoryPlugin).AssemblyQualifiedNameWithoutVersion());

            return new CustomSchedulerFactory(x, schedulerBuilder.Properties);
        });
        builder.Services.AddSingleton<IJobFactory, ServiceProviderJobFactory>();

        var appRunTempRootPath = builder.Configuration["ConsoleAppPackageRunTempPath"] ?? AppDomain.CurrentDomain.BaseDirectory;

        builder.Services.AddDbContext<SettingsDbContext>(o => o.UseNpgsql(builder.Configuration["ConnectionString"]));

        builder.Services.AddScoped<IConsoleAppPackageRunner>(x => new DefaultConsoleAppPackageRunner(
            x.GetRequiredService<IAsyncPublisher<JobConsoleLogMessageEvent>>(),
            x.GetRequiredService<IEmailSender>(),
            appRunTempRootPath));
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<IEmailSender>(x => new SmtpEmailSender(x.GetRequiredService<ISettingsService>()));
        builder.Services.AddTransient<ConsoleAppPackageJob>();

        builder.Services.AddMessagePipe(
            x =>
            {
                x.InstanceLifetime = InstanceLifetime.Singleton;
                x.RequestHandlerLifetime = InstanceLifetime.Singleton;
                x.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
                x.EnableAutoRegistration = false;
            });

        builder.Services.AddSingleton(x => x.GetRequiredService<ISchedulerFactory>().GetScheduler().Result);
        builder.Services.AddSingleton<ISchedulerManager, SchedulerManager>();
        builder.Services.AddScoped<ISchedulerService, SchedulerService>();

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

        _app.MapHub<JobRunConsoleHub>("/jobRunConsoleHub");

        _app.MapWhen(c => c.Request.Path.StartsWithSegments("/api"), b =>
        {
            b.UseRouting();
            b.UseAuthorization();
            b.UseEndpoints(e => e.MapControllers());
        });

        _app.MapWhen(c => !c.Request.Path.StartsWithSegments("/api"), b =>
        {
            b.UseDefaultFiles();
            b.UseStaticFiles();
            b.UseRouting();
            b.UseAuthorization();
            b.UseEndpoints(e => e.MapFallbackToFile("index.html"));
        });

        _schedulerManager = _app.Services.GetRequiredService<ISchedulerManager>();
        _schedulerManager.SubscribeToEvent(_app.Services.GetRequiredService<JobConsoleLogMessageToHubHandler>());

        await AddInitialData(_app).ConfigureAwait(false);

        await _schedulerManager.Start(_app.Services.GetRequiredService<ILoggerFactory>());
        await _app.RunAsync().ConfigureAwait(false);
    }

    private static async Task AddInitialData(IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<int>>>();
        using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await using var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityManagementDbContext>();

        await identityDbContext.Database.MigrateAsync().ConfigureAwait(false);

        var roles = new[] { Roles.Admin, Roles.JobEditor, Roles.JobViewer };
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
            adminUser = new IdentityUser<int>("admin") { Email = "admin@email.com" };

            await userManager.CreateAsync(adminUser, "Password");
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}