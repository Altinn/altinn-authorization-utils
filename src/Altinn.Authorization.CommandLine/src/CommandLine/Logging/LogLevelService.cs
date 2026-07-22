using System.Collections.Frozen;
using System.CommandLine;
using System.CommandLine.Parsing;
using Altinn.Authorization.CommandLine.Help;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Logging;

internal sealed class LogLevelService
    : IConfigureOptions<LoggerFilterOptions>
    , IOptionsChangeTokenSource<LoggerFilterOptions>
{
    const LogLevel DefaultLogLevel = LogLevel.Information;
    private static readonly FrozenDictionary<string, LogLevel> VerbosityValues
        = FrozenDictionary.Create<string, LogLevel>(
        StringComparer.OrdinalIgnoreCase,
        [
            new("q", LogLevel.Critical),
            new("quiet", LogLevel.Critical),
            new("m", LogLevel.Warning),
            new("minimal", LogLevel.Warning),
            new("n", LogLevel.Information),
            new("normal", LogLevel.Information),
            new("d", LogLevel.Debug),
            new("detailed", LogLevel.Debug),
            new("diag", LogLevel.Trace),
            new("diagnostic", LogLevel.Trace),
        ]);

    private readonly Option<LogLevel> _verbosityOption;
    private readonly Option<byte> _verboseOption;
    private readonly Option<byte> _quietOption;

    private LogLevel? _verbosity;
    private CancellationTokenSource? _cancellationTokenSource;

    public LogLevelService()
    {
        _verbosityOption = new Option<LogLevel>("--verbosity")
        {
            Description = "Set the log verbosity - mutually exclusive with --verbose and --quiet",
            Arity = ArgumentArity.ZeroOrOne,
            Recursive = true,
            HelpCustomization =
            {
                DisplayArgument = HelpDisplayArgumentCustomization.Create([
                    "q[uiet]",
                    "m[inimal]",
                    "n[ormal]",
                    "d[etailed]",
                    "diag[nostic]",
                ]),
                GetDefaultValue = () => ["normal"],
            },
        };

        _verboseOption = new Option<byte>("--verbose", "-v")
        {
            Description = "Increment the log verbosity - mutually exclusive with --verbosity and --quiet",
            Arity = new ArgumentArity(0, 2),
            Recursive = true,
            HelpCustomization =
            {
                DisplayArgument = false,
            },
        };

        _quietOption = new Option<byte>("--quiet", "-q")
        {
            Description = "Decrement the log verbosity - mutually exclusive with --verbosity and --verbose",
            Arity = new ArgumentArity(0, 3),
            Recursive = true,
            HelpCustomization =
            {
                DisplayArgument = false,
            },
        };

        _verboseOption.CustomParser = _quietOption.CustomParser = GetVerboseQuietCount;
        _verbosityOption.ParserAndValueFactory = GetVerbosityLevel;
    }

    internal void Configure(ICommandConventionBuilder builder)
    {
        builder.Add(builder =>
        {
            builder.Options.Add(_verbosityOption);
            builder.Options.Add(_verboseOption);
            builder.Options.Add(_quietOption);

            builder.Validators.Add(commandResult =>
            {
                var verbosity = commandResult.GetResult(_verbosityOption);
                var verbose = commandResult.GetResult(_verboseOption);
                var quiet = commandResult.GetResult(_quietOption);

                var nonImplicit = 0;
                if (verbosity is { Implicit: false }) nonImplicit++;
                if (verbose is { Implicit: false }) nonImplicit++;
                if (quiet is { Implicit: false }) nonImplicit++;

                if (nonImplicit > 1)
                {
                    commandResult.AddError("The --verbosity, --verbose, and --quiet options are mutually exclusive.");
                }
            });
        });

        builder.Finally(builder =>
        {
            builder.Middleware.Insert(0, (context, next, cancellationToken) =>
            {
                _verbosity = GetVerbosity(context.ParseResult);
                FireChangeToken();

                return next(context, cancellationToken);
            });
        }, recursive: true);
    }

    string? IOptionsChangeTokenSource<LoggerFilterOptions>.Name
        => Options.DefaultName;

    void IConfigureOptions<LoggerFilterOptions>.Configure(LoggerFilterOptions options)
    {
        options.MinLevel = _verbosity ?? DefaultLogLevel;
    }

    IChangeToken IOptionsChangeTokenSource<LoggerFilterOptions>.GetChangeToken()
    {
        CancellationTokenSource cts = LazyInitializer.EnsureInitialized(ref _cancellationTokenSource, () => new CancellationTokenSource());
        return new CancellationChangeToken(cts.Token);
    }

    private void FireChangeToken()
    {
        CancellationTokenSource? tcs = Interlocked.Exchange(ref _cancellationTokenSource, null);
        tcs?.Cancel();
    }

    private LogLevel GetVerbosity(ParseResult parseResult)
    {
        var verbosity = parseResult.GetResult(_verbosityOption);
        var verbose = parseResult.GetResult(_verboseOption);
        var quiet = parseResult.GetResult(_quietOption);

        if (verbosity is { Implicit: false })
        {
            return verbosity.GetValueOrDefault<LogLevel>();
        }

        if (verbose is { Implicit: false })
        {
            var verboseCount = verbose.GetValueOrDefault<byte>();
            return verboseCount switch
            {
                0 => LogLevel.Information,
                1 => LogLevel.Debug,
                _ => LogLevel.Trace,
            };
        }

        if (quiet is { Implicit: false })
        {
            var quietCount = quiet.GetValueOrDefault<byte>();
            return quietCount switch
            {
                0 => LogLevel.Information,
                1 => LogLevel.Warning,
                2 => LogLevel.Error,
                _ => LogLevel.Critical,
            };
        }

        return DefaultLogLevel;
    }

    private static byte GetVerboseQuietCount(ArgumentResult result)
    {
        var optionResult = (OptionResult)result.Parent!;
        return checked((byte)optionResult.IdentifierTokenCount);
    }

    private static LogLevel GetVerbosityLevel(ArgumentResult result)
    {
        if (result.Tokens.Count == 0)
        {
            return DefaultLogLevel;
        }

        if (result.Tokens.Count > 1)
        {
            result.AddError("The --verbosity option can only be specified once.");
            return DefaultLogLevel;
        }

        var token = result.Tokens[0].Value;
        if (!VerbosityValues.TryGetValue(token, out var logLevel))
        {
            result.AddError($"Argument '{token}' not recognized. Must be one of: 'q[uiet]', 'm[inimal]', 'n[ormal]', 'd[etailed]', 'diag[nostic]'");
            return DefaultLogLevel;
        }

        return logLevel;
    }
}
