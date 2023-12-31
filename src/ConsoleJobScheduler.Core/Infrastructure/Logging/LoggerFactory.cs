﻿using Microsoft.Extensions.Logging;

using Quartz.Logging;

namespace ConsoleJobScheduler.Core.Infrastructure.Logging;

public static class LoggerFactory
{
    private static ILoggerFactory? _loggerFactory;

    public static void SetLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        LogContext.SetCurrentLogProvider(loggerFactory);
    }

    public static ILogger<T> CreateLogger<T>()
    {
        return (_loggerFactory ?? throw new InvalidOperationException("Logger factory cannot be null")).CreateLogger<T>();
    }
}