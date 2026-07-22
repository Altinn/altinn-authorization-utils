using System.CommandLine;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Altinn.Authorization.CommandLine.Logging;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.XmlDoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// A builder for creating a <see cref="CliApplication"/> instance.
/// </summary>
public sealed class CliApplicationBuilder
    : IHostApplicationBuilder
{
    /// <summary>
    /// Creates a new <see cref="CliApplicationBuilder"/> instance with the specified application name.
    /// </summary>
    /// <param name="description">The description of the application.</param>
    /// <returns>A <see cref="CliApplicationBuilder"/> instance.</returns>
    internal static CliApplicationBuilder Create(string description)
    {
        var executableName = RootCommand.ExecutableName;
        var hostBuilder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = null, // do not pass args here, as it will be used by the Host to configure itself.
            EnvironmentName = "cli",
            ApplicationName = executableName,
            ContentRootPath = null,
        });

        hostBuilder.Services.AddSingleton<IHelpBuilder, DefaultHelpBuilder>();
        hostBuilder.Services.AddSingleton<ExtendedHelpAction>();
        hostBuilder.Services.AddOptions<InvocationConfiguration>()
            .Configure((InvocationConfiguration cfg, IConsole console) =>
            {
                cfg.ProcessTerminationTimeout = TimeSpan.FromSeconds(10);
                cfg.EnableDefaultExceptionHandler = false;
                cfg.Output = new ConsoleWriter(console.StdOut);
                cfg.Error = new ConsoleWriter(console.StdErr);
            });

        hostBuilder.Services.AddSingleton<OptionFactory, DefaultOptionFactory>();
        hostBuilder.Services.AddSingleton<ArgumentFactory, DefaultArgumentFactory>();

        // documentation generation
        hostBuilder.Services.AddSingleton<IXmlDocProvider, DefaultXmlDocProvider>();

        // result handlers
        hostBuilder.Services.AddSingleton<ICommandResultHandler<int>, IntResultHandler>();

        // console
        hostBuilder.Services.AddSingleton<IExclusivityMode, SharedExclusivityMode>();
        hostBuilder.Services.AddSingleton<IConsole, Console.Console>();
        hostBuilder.Services.AddSingleton(s => (IAnsiConsole)s.GetRequiredService<IConsole>());
        hostBuilder.Services.AddKeyedSingleton(serviceKey: ConsoleTarget.StdOut, (s, _) => s.GetRequiredService<IConsole>().StdOut);
        hostBuilder.Services.AddKeyedSingleton(serviceKey: ConsoleTarget.StdErr, (s, _) => s.GetRequiredService<IConsole>().StdErr);

        // logging
        hostBuilder.Services.TryAddSingleton(TimeProvider.System);
        hostBuilder.Logging.ClearProviders();
        hostBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<AnsiConsoleFormatter, SimpleAnsiConsoleFormatter>());
        hostBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AnsiConsoleLoggerProvider>());
        hostBuilder.Services.TryAddSingleton<InvocationLogger>();
        ConfigureLogLevel(hostBuilder);
        hostBuilder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

        return new CliApplicationBuilder(hostBuilder, description);

        static void ConfigureLogLevel(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<LogLevelService>();
            builder.Services.AddSingleton<IConfigureOptions<LoggerFilterOptions>>(s => s.GetRequiredService<LogLevelService>());
            builder.Services.AddSingleton<IOptionsChangeTokenSource<LoggerFilterOptions>>(s => s.GetRequiredService<LogLevelService>());
        }
    }

    private readonly HostApplicationBuilder _hostBuilder;
    private readonly string _description;

    private CliApplicationBuilder(HostApplicationBuilder hostBuilder, string description)
    {
        _hostBuilder = hostBuilder;
        _description = description;
    }

    /// <inheritdoc/>
    IDictionary<object, object> IHostApplicationBuilder.Properties
        => ((IHostApplicationBuilder)_hostBuilder).Properties;

    /// <inheritdoc/>
    IConfigurationManager IHostApplicationBuilder.Configuration
        => ((IHostApplicationBuilder)_hostBuilder).Configuration;

    /// <inheritdoc/>
    public ConfigurationManager Configuration => _hostBuilder.Configuration;

    /// <inheritdoc/>
    public IHostEnvironment Environment => _hostBuilder.Environment;

    /// <inheritdoc/>
    public ILoggingBuilder Logging => _hostBuilder.Logging;

    /// <inheritdoc/>
    public IMetricsBuilder Metrics => _hostBuilder.Metrics;

    /// <inheritdoc/>
    public IServiceCollection Services => _hostBuilder.Services;

    /// <summary>
    /// Builds the <see cref="CliApplication"/> instance.
    /// </summary>
    /// <returns>A <see cref="CliApplication"/> instance.</returns>
    public CliApplication Build()
    {
        var host = _hostBuilder.Build();
        var console = host.Services.GetRequiredService<IConsole>();

        return new CliApplication(_description, host, console);
    }

    /// <inheritdoc/>
    void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure)
    {
        ((IHostApplicationBuilder)_hostBuilder).ConfigureContainer(factory, configure);
    }
}
