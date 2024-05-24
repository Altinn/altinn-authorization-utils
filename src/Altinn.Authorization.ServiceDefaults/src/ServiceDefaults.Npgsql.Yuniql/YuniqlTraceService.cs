using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Yuniql.Extensibility;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

internal partial class YuniqlTraceService 
    : ITraceService
{
    private readonly ILogger<YuniqlTraceService> _logger;

    public YuniqlTraceService(ILogger<YuniqlTraceService> logger)
    {
        _logger = logger;
    }

    public bool IsDebugEnabled { get => _logger.IsEnabled(LogLevel.Debug); set => ThrowHelper.ThrowNotSupportedException<bool>(); }
    public bool IsTraceSensitiveData { get => false; set => ThrowHelper.ThrowNotSupportedException<bool>(); }
    public bool IsTraceToDirectory { get => false; set => ThrowHelper.ThrowNotSupportedException<bool>(); }
    public bool IsTraceToFile { get => false; set => ThrowHelper.ThrowNotSupportedException<bool>(); }
    public string? TraceDirectory { get => null; set => ThrowHelper.ThrowNotSupportedException<bool>(); }

    public void Debug(string message, object? payload = null)
    {
        Log.Debug(_logger, message, payload);
    }

    public void Error(string message, object? payload = null)
    {
        Log.Error(_logger, message, payload);
    }

    public void Info(string message, object? payload = null)
    {
        Log.Info(_logger, message, payload);
    }

    public void Success(string message, object? payload = null)
    {
        Log.Success(_logger, message, payload);
    }

    public void Warn(string message, object? payload = null)
    {
        Log.Warn(_logger, message, payload);
    }

    [SuppressMessage(
        "LoggerMessage", 
        "LOGGEN036:A value being logged doesn't have an effective way to be converted into a string", 
        Justification = "We can't really do anything more than ToString on these objects we get from yuniql")]
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Debug, "Yuniql: {Message}. {Payload}")]
        public static partial void Debug(ILogger logger, string message, object? payload);

        [LoggerMessage(2, LogLevel.Error, "Yuniql: {Message}. {Payload}")]
        public static partial void Error(ILogger logger, string message, object? payload);

        [LoggerMessage(3, LogLevel.Information, "Yuniql: {Message}. {Payload}")]
        public static partial void Info(ILogger logger, string message, object? payload);

        [LoggerMessage(4, LogLevel.Information, "Yuniql: {Message}. {Payload}")]
        public static partial void Success(ILogger logger, string message, object? payload);

        [LoggerMessage(5, LogLevel.Warning, "Yuniql: {Message}. {Payload}")]
        public static partial void Warn(ILogger logger, string message, object? payload);
    }
}
