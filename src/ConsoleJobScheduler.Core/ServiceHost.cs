﻿using ConsoleJobScheduler.Core.Api.Hubs;
using ConsoleJobScheduler.Core.Api.Hubs.Handlers;
using ConsoleJobScheduler.Core.Application;
using ConsoleJobScheduler.Core.Application.Module;
using ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;
using ConsoleJobScheduler.Core.Infra.EMail;
using MessagePipe;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddSignalR();
        builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

        builder.Services.AddSingleton<JobConsoleLogMessageToHubHandler>();

        builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, option =>
            {
                option.LoginPath = new PathString("/api/auth/Login");
                option.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
            });

        var identityModule = new IdentityModule(builder.Configuration);
        var schedulerModule = new SchedulerModule(builder.Configuration);
        var historyModule = new JobHistoryModule(builder.Configuration);
        var jobRunModule = new JobRunModule(builder.Configuration);
        var settingsModule = new SettingsModule(builder.Configuration);

        identityModule.Register(builder.Services);
        schedulerModule.Register(builder.Services);
        historyModule.Register(builder.Services);
        jobRunModule.Register(builder.Services);
        settingsModule.Register(builder.Services);

        builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

        builder.Services.AddMessagePipe(
            x =>
            {
                x.InstanceLifetime = InstanceLifetime.Singleton;
                x.RequestHandlerLifetime = InstanceLifetime.Singleton;
                x.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
                x.EnableAutoRegistration = false;
            });

        _app = builder.Build();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        _app.Lifetime.ApplicationStopped.Register(
                () =>
                {
                    if (!_stopRequested)
                    {
                        Stop().GetAwaiter().GetResult();
                    }
                });

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

        await identityModule.MigrateDb(_app.Services).ConfigureAwait(false);
        await historyModule.MigrateDb(_app.Services).ConfigureAwait(false);
        schedulerModule.MigrateDb();
        await settingsModule.MigrateDb(_app.Services).ConfigureAwait(false);
        await jobRunModule.MigrateDb(_app.Services).ConfigureAwait(false);

        using var scope = _app.Services.CreateScope();
        var identityApplicationService = scope.ServiceProvider.GetRequiredService<IIdentityApplicationService>();
        await identityApplicationService.AddInitialRolesAndUsers().ConfigureAwait(false);

        _schedulerManager = _app.Services.GetRequiredService<ISchedulerManager>();
        _schedulerManager.SubscribeToEvent(_app.Services.GetRequiredService<JobConsoleLogMessageToHubHandler>());

        await _schedulerManager.Start();
        await _app.RunAsync().ConfigureAwait(false);
    }
}