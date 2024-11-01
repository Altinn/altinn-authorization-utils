using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using System.Collections.Concurrent;
using Yuniql.Extensibility;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

/// <summary>
/// Extension methods for <see cref="INpgsqlDatabaseBuilder"/>.
/// </summary>
public static class YuniqlDatabaseMigratorExtensions
{
    private static readonly ObjectFactory<YuniqlDatabaseMigrator> _factory
        = ActivatorUtilities.CreateFactory<YuniqlDatabaseMigrator>([typeof(YuniqlDatabaseMigrator.Settings)]);

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">Configuration delegate for configuring Yuniql migrations.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        Action<IServiceProvider, YuniqlDatabaseMigratorOptions> configure)
        => AddYuniqlMigrations(builder, serviceKey: null, configure);

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="serviceKey">Unique key for this yuniql migration - allows for multiple migrations to exist.</param>
    /// <param name="configure">Configuration delegate for configuring Yuniql migrations.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        object? serviceKey,
        Action<IServiceProvider, YuniqlDatabaseMigratorOptions> configure)
    {
        var name = Keys.GetName(serviceKey);
        var keyMarker = Markers.ServiceDescriptorFor(serviceKey);

        builder.Services.AddOptions<YuniqlDatabaseMigratorOptions>(name)
            .Bind(builder.Configuration.GetSection("Yuniql"))
            .Configure((YuniqlDatabaseMigratorOptions opts, IServiceProvider services) =>
            {
                configure(services, opts);
            });

        if (builder.Services.Contains(keyMarker))
        {
            return builder;
        }

        builder.Services.Add(keyMarker);
        var settings = new YuniqlDatabaseMigrator.Settings { Key = serviceKey, Name = name };
        builder.Services.AddSingleton<INpgsqlDatabaseMigrator>(sp => _factory(sp, [settings]));

        if (builder.Services.Contains(Markers.SharedServiceDescriptor))
        {
            return builder;
        }

        builder.Services.Add(Markers.SharedServiceDescriptor);
        builder.Services.AddLogging();
        builder.Services.TryAddSingleton<ITraceService, YuniqlTraceService>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<YuniqlDatabaseMigratorOptions>, ConfigureYuniqlEnvironmentFromHostEnvironment>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<YuniqlDatabaseMigratorOptions>, ConfigureYuniqlTokensFromDatabaseMigrationOptions>());
        builder.Services.ConfigureOpenTelemetryTracerProvider((services, builder) =>
        {
            var options = services.GetRequiredService<IOptions<YuniqlDatabaseMigratorOptions>>().Value;
            if (!options.DisableTracing)
            {
                builder.AddSource(YuniqlActivityProvider.SourceName);
            }
        });

        return builder;
    }

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">Configuration delegate for configuring Yuniql migrations.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        Action<YuniqlDatabaseMigratorOptions> configure)
        => AddYuniqlMigrations(builder, (_, opts) => configure(opts));

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="serviceKey">Unique key for this yuniql migration - allows for multiple migrations to exist.</param>
    /// <param name="configure">Configuration delegate for configuring Yuniql migrations.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        object? serviceKey,
        Action<YuniqlDatabaseMigratorOptions> configure)
        => AddYuniqlMigrations(builder, serviceKey, (_, opts) => configure(opts));

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="fileProvider">The <see cref="IFileProvider"/> used as <see cref="YuniqlDatabaseMigratorOptions.WorkspaceFileProvider"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        IFileProvider fileProvider)
        => AddYuniqlMigrations(builder, (opts) => opts.WorkspaceFileProvider = fileProvider);

    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="serviceKey">Unique key for this yuniql migration - allows for multiple migrations to exist.</param>
    /// <param name="fileProvider">The <see cref="IFileProvider"/> used as <see cref="YuniqlDatabaseMigratorOptions.WorkspaceFileProvider"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        object? serviceKey,
        IFileProvider fileProvider)
        => AddYuniqlMigrations(builder, serviceKey, (opts) => opts.WorkspaceFileProvider = fileProvider);

    private sealed class ConfigureYuniqlEnvironmentFromHostEnvironment
        : IConfigureNamedOptions<YuniqlDatabaseMigratorOptions>
    {
        private readonly IHostEnvironment _hostEnvironment;

        public ConfigureYuniqlEnvironmentFromHostEnvironment(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public void Configure(string? name, YuniqlDatabaseMigratorOptions options)
            => Configure(options);

        public void Configure(YuniqlDatabaseMigratorOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Environment))
            {
                options.Environment = _hostEnvironment.EnvironmentName;
            }
        }
    }

    private sealed class ConfigureYuniqlTokensFromDatabaseMigrationOptions
        : IConfigureNamedOptions<YuniqlDatabaseMigratorOptions>
    {
        private readonly IOptionsMonitor<NpgsqlDatabaseMigrationOptions> _inner;

        public ConfigureYuniqlTokensFromDatabaseMigrationOptions(IOptionsMonitor<NpgsqlDatabaseMigrationOptions> inner)
        {
            _inner = inner;
        }

        public void Configure(string? name, YuniqlDatabaseMigratorOptions options)
        {
            Configure(options);
        }

        public void Configure(YuniqlDatabaseMigratorOptions options)
        {
            var innerOptions = _inner.CurrentValue;

            Assign(options, innerOptions);
        }

        private static void Assign(YuniqlDatabaseMigratorOptions options, NpgsqlDatabaseMigrationOptions innerOptions)
        {
            if (!string.IsNullOrEmpty(innerOptions.MigratorRole))
            {
                options.Tokens.TryAdd("YUNIQL-USER", innerOptions.MigratorRole);
            }

            if (!string.IsNullOrEmpty(innerOptions.AppRole))
            {
                options.Tokens.TryAdd("APP-USER", innerOptions.AppRole);
            }
        }
    }

    private static class Markers
    {
        private static readonly ServiceDescriptor _defaultDescriptor = ServiceDescriptor.Singleton<YuniqlServiceMaker, YuniqlServiceMaker>();
        private static readonly ConcurrentDictionary<object, ServiceDescriptor>
            _descriptors = new();

        public static ServiceDescriptor ServiceDescriptorFor(object? serviceKey)
        {
            if (serviceKey is null)
            {
                return _defaultDescriptor;
            }

            return _descriptors.GetOrAdd(serviceKey, static key => ServiceDescriptor.KeyedSingleton<YuniqlServiceMaker, YuniqlServiceMaker>(key));
        }

        public static ServiceDescriptor SharedServiceDescriptor { get; }
            = ServiceDescriptor.Singleton<YuniqlSharedServiceMaker, YuniqlSharedServiceMaker>();

        private sealed class YuniqlServiceMaker { }
        private sealed class YuniqlSharedServiceMaker { }
    }

    private static class Keys
    {
        private static readonly ConcurrentDictionary<object, string>
            _names = new();

        public static string? GetName(object? serviceKey)
        {
            if (serviceKey is null)
            {
                return null;
            }

            return _names.GetOrAdd(serviceKey, static serviceKey => $"yuniql-key:{Guid.NewGuid()}:{serviceKey}");
        }
    }
}
