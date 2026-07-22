using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Console;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.CommandLine.Logging;

[ProviderAlias("AnsiConsole")]
internal sealed class AnsiConsoleLoggerProvider
    : ILoggerProvider
    , ISupportExternalScope
{
    private readonly IOptionsMonitor<AnsiConsoleLoggerOptions> _options;
    private readonly ConcurrentDictionary<string, AnsiConsoleLogger> _loggers;
    private readonly IConsole _console;
    private ConcurrentDictionary<string, AnsiConsoleFormatter> _formatters;

    private readonly IDisposable? _optionsReloadToken;
    private IExternalScopeProvider? _scopeProvider = null;

    /// <summary>
    /// Creates an instance of <see cref="AnsiConsoleLoggerProvider"/>.
    /// </summary>
    /// <param name="options">The options to create <see cref="AnsiConsoleLogger"/> instances with.</param>
    /// <param name="formatters">Log formatters added for <see cref="AnsiConsoleLogger"/> instances.</param>
    /// <param name="console">The console to write log messages to.</param>
    public AnsiConsoleLoggerProvider(
        IOptionsMonitor<AnsiConsoleLoggerOptions> options,
        IEnumerable<AnsiConsoleFormatter>? formatters,
        IConsole console)
    {
        _console = console;
        _options = options;
        _loggers = new ConcurrentDictionary<string, AnsiConsoleLogger>();
        SetFormatters(formatters);

        ReloadLoggerOptions(options.CurrentValue);
        _optionsReloadToken = _options.OnChange(ReloadLoggerOptions);
    }

    [MemberNotNull(nameof(_formatters))]
    private void SetFormatters(IEnumerable<AnsiConsoleFormatter>? formatters = null)
    {
        var cd = new ConcurrentDictionary<string, AnsiConsoleFormatter>(StringComparer.OrdinalIgnoreCase);

        bool added = false;
        if (formatters != null)
        {
            foreach (AnsiConsoleFormatter formatter in formatters)
            {
                cd.TryAdd(formatter.Name, formatter);
                added = true;
            }
        }

        if (!added)
        {
            ThrowHelper.ThrowArgumentException("At least one formatter must be provided.", nameof(formatters));
        }

        _formatters = cd;
    }

    // warning:  ReloadLoggerOptions can be called before the ctor completed,... before registering all of the state used in this method need to be initialized
    private void ReloadLoggerOptions(AnsiConsoleLoggerOptions options)
    {
        if (options.FormatterName == null || !_formatters.TryGetValue(options.FormatterName, out AnsiConsoleFormatter? logFormatter))
        {
            logFormatter = _formatters[ConsoleFormatterNames.Simple];
        }

        foreach (KeyValuePair<string, AnsiConsoleLogger> logger in _loggers)
        {
            logger.Value.Options = options;
            logger.Value.Formatter = logFormatter;
        }
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name)
    {
        if (_options.CurrentValue.FormatterName == null || !_formatters.TryGetValue(_options.CurrentValue.FormatterName, out AnsiConsoleFormatter? logFormatter))
        {
            logFormatter = _formatters[ConsoleFormatterNames.Simple];
        }

        return _loggers.TryGetValue(name, out AnsiConsoleLogger? logger)
            ? logger
            : _loggers.GetOrAdd(name, new AnsiConsoleLogger(name, _console, logFormatter, _scopeProvider, _options.CurrentValue));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    /// <inheritdoc />
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;

        foreach (KeyValuePair<string, AnsiConsoleLogger> logger in _loggers)
        {
            logger.Value.ScopeProvider = _scopeProvider;
        }
    }
}
