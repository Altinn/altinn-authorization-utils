using Altinn.Authorization.CommandLine.Console;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Altinn.Authorization.CommandLine.Logging;

internal sealed class AnsiConsoleLogger
    : ILogger
{
    private readonly string _name;
    private readonly IConsole _console;

    internal AnsiConsoleLogger(
        string name,
        IConsole console,
        AnsiConsoleFormatter formatter,
        IExternalScopeProvider? scopeProvider,
        AnsiConsoleLoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);

        _name = name;
        _console = console;
        Formatter = formatter;
        ScopeProvider = scopeProvider;
        Options = options;
    }

    internal AnsiConsoleFormatter Formatter { get; set; }
    internal IExternalScopeProvider? ScopeProvider { get; set; }
    internal AnsiConsoleLoggerOptions Options { get; set; }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(formatter);
        LogEntry<TState> logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
        var renderable = Formatter.Format(in logEntry, ScopeProvider);

        _console.ExclusivityMode.Run(() =>
        {
            _console.StdErr.Write(renderable);
            return 0;
        });
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
        => ScopeProvider?.Push(state) ?? NullScope.Instance;

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <summary>
    /// An empty scope without any logic
    /// </summary>
    private sealed class NullScope
        : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
