using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Yuniql.Extensibility;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

/// <summary>
/// Extension methods for <see cref="INpgsqlDatabaseBuilder"/>.
/// </summary>
public static class YuniqlDatabaseMigratorExtensions
{
    /// <summary>
    /// Adds Yuniql migrations to the <see cref="INpgsqlDatabaseBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">Configuration delegate for configuring Yuniql migrations.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddYuniqlMigrations(
        this INpgsqlDatabaseBuilder builder,
        Action<IServiceProvider, YuniqlDatabaseMigratorOptions> configure)
    {
        builder.Services.AddOptions<YuniqlDatabaseMigratorOptions>()
            .Configure((YuniqlDatabaseMigratorOptions opts, IServiceProvider services) =>
            {
                configure(services, opts);
            });

        if (builder.Services.Any(s => s.ServiceType == typeof(Marker)))
        {
            return builder;
        }

        builder.Services.AddSingleton<Marker>();
        builder.Services.AddLogging();
        builder.Services.TryAddSingleton<ITraceService, YuniqlTraceService>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<YuniqlDatabaseMigratorOptions>, ConfigureYuniqlEnvironmentFromHostEnvironment>());
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<YuniqlDatabaseMigratorOptions>, ConfigureYuniqlTokensFromDatabaseMigrationOptions>());

        builder.Services.AddSingleton<INpgsqlDatabaseMigrator, YuniqlDatabaseMigrator>();

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
            var innerOptions = _inner.Get(name);

            Assign(options, innerOptions);
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

    private sealed class Marker
    {
    }
}
