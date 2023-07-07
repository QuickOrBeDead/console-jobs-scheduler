namespace ConsoleJobScheduler.WindowsService;

using ConsoleJobScheduler.WindowsService.Hubs;
using ConsoleJobScheduler.WindowsService.Hubs.Handlers;
using ConsoleJobScheduler.WindowsService.Scheduler;

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

        _app.UseStaticFiles();
        _app.UseRouting();
        _app.UseAuthorization();
        _app.MapControllers();
        _app.MapHub<JobRunConsoleHub>("/jobRunConsoleHub");

        _app.UseSpaStaticFiles();
        _app.UseSpa(configuration: b =>
            {
                if (_app.Environment.IsDevelopment())
                {
                    b.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                }
            });

        schedulerService.SubscribeToEvent(_app.Services.GetRequiredService<JobConsoleLogMessageToHubHandler>());

        await schedulerService.Start(_app.Services.GetRequiredService<ILoggerFactory>());
        await _app.RunAsync(_app.Configuration["WebAppUrl"]).ConfigureAwait(false);
    }
}