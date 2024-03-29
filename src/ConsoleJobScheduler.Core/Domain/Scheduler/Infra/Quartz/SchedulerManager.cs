﻿using ConsoleJobScheduler.Core.Domain.Runner.Events;
using MessagePipe;
using Quartz;

namespace ConsoleJobScheduler.Core.Domain.Scheduler.Infra.Quartz;

public interface ISchedulerManager
{
    Task Start();

    Task Shutdown();

    void SubscribeToEvent(IAsyncMessageHandler<JobConsoleLogMessageEvent> handler);
}

public sealed class SchedulerManager : ISchedulerManager
{
    private readonly IAsyncSubscriber<JobConsoleLogMessageEvent> _subscriber;
    private readonly IScheduler _scheduler;
    private readonly DisposableBagBuilder _subscriberDisposableBagBuilder;

    public SchedulerManager(IAsyncSubscriber<JobConsoleLogMessageEvent> subscriber, IScheduler scheduler)
    {
        _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _subscriberDisposableBagBuilder = DisposableBag.CreateBuilder();
    }

    public Task Start()
    {
        return _scheduler.Start();
    }

    public Task Shutdown()
    {
        _subscriberDisposableBagBuilder.Build().Dispose();

        return _scheduler.Shutdown();
    }

    public void SubscribeToEvent(IAsyncMessageHandler<JobConsoleLogMessageEvent> handler)
    {
        _subscriber.Subscribe(handler).AddTo(_subscriberDisposableBagBuilder);
    }
}