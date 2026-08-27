using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Logging;

internal sealed class SimpleAnsiConsoleFormatter
    : AnsiConsoleFormatter
    , IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private readonly TimeProvider _timeProvider;

    public SimpleAnsiConsoleFormatter(
        TimeProvider timeProvider,
        IOptionsMonitor<SimpleConsoleFormatterOptions> options)
        : base(ConsoleFormatterNames.Simple)
    {
        _timeProvider = timeProvider;

        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(FormatterOptions))]
    private void ReloadLoggerOptions(SimpleConsoleFormatterOptions options)
    {
        FormatterOptions = options;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    internal SimpleConsoleFormatterOptions FormatterOptions { get; set; }

    public sealed override IRenderable Format<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            string message = bufferedRecord.FormattedMessage ?? string.Empty;
            IRenderable? exception = logEntry.Exception?.GetRenderable();

            if (exception is null && !string.IsNullOrEmpty(bufferedRecord.Exception))
            {
                exception = new Text(bufferedRecord.Exception);
            }

            return WriteInternal(null, message, bufferedRecord.LogLevel, bufferedRecord.EventId.Id, exception, logEntry.Category, bufferedRecord.Timestamp);
        }
        else
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return Text.Empty;
            }

            // We extract most of the work into a non-generic method to save code size. If this was left in the generic
            // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
            return WriteInternal(scopeProvider, message, logEntry.LogLevel, logEntry.EventId.Id, logEntry.Exception?.GetRenderable(), logEntry.Category, GetCurrentDateTime());
        }
    }

    private IRenderable WriteInternal(
        IExternalScopeProvider? scopeProvider,
        string message,
        LogLevel logLevel,
        int eventId,
        IRenderable? exception,
        string category,
        DateTimeOffset stamp)
    {
        var logLevelColors = GetLogLevelConsoleColors(logLevel);
        string logLevelString = GetLogLevelString(logLevel);

        bool singleLine = FormatterOptions.SingleLine;
        Paragraph header = new();

        // Example:
        // 00:00:00 info: ConsoleApp.Program[10]
        //     Request received

        string? timestampFormat = FormatterOptions.TimestampFormat;
        if (timestampFormat != null)
        {
            header.Append(stamp.ToString(timestampFormat));
        }

        header.Append(logLevelString, logLevelColors);
        header.Append(": ");
        header.Append(category); // todo: color?
        header.Append("[");
        header.Append(eventId.ToString()); // todo: color?
        header.Append("]");

        Paragraph bodyParagraph = new();

        // scope information
        WriteScopeInformation(bodyParagraph, scopeProvider);
        bodyParagraph.Append(message);
        IRenderable body = bodyParagraph;

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception != null)
        {
            body = new Rows(body, exception);
        }

        body = new Padder(body, new Padding(left: 6, top: 0, right: 0, bottom: 0));
        return new Rows(header, body);
    }

    private void WriteScopeInformation(Paragraph paragraph, IExternalScopeProvider? scopeProvider)
    {
        if (FormatterOptions.IncludeScopes && scopeProvider != null)
        {
            scopeProvider.ForEachScope(
                static (scope, paragraph) =>
                {
                    paragraph.Append(" => ");
                    string? scopeMessage = scope?.ToString(); // ConsoleControlCharacterSanitizer.Sanitize(scope?.ToString());
                    paragraph.Append(scopeMessage ?? "");
                },
                paragraph);
        }
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        if (FormatterOptions.TimestampFormat is null)
        {
            return DateTimeOffset.MinValue;
        }

        var utcNow = _timeProvider.GetUtcNow();
        return FormatterOptions.UseUtcTimestamp ? utcNow : utcNow.ToLocalTime();
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(logLevel)),
        };
    }

    private Style GetLogLevelConsoleColors(LogLevel logLevel)
    {
        // We shouldn't be outputting color codes for Android/Apple mobile platforms,
        // they have no shell (adb shell is not meant for running apps) and all the output gets redirected to some log file.
        if (FormatterOptions.ColorBehavior == LoggerColorBehavior.Disabled)
        {
            return Style.Plain;
        }

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new Style(foreground: Color.Gray, background: null),
            LogLevel.Debug => new Style(foreground: Color.Gray, background: null),
            LogLevel.Information => new Style(foreground: Color.DarkGreen, background: null),
            LogLevel.Warning => new Style(foreground: Color.Yellow, background: null),
            LogLevel.Error => new Style(foreground: Color.Black, background: Color.DarkRed),
            LogLevel.Critical => new Style(foreground: Color.White, background: Color.DarkRed),
            _ => Style.Plain,
        };
    }
}
