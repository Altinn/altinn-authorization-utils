using Altinn.Authorization.ServiceDefaults.Npgsql;
using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using CommunityToolkit.Diagnostics;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Host builder extensions for Npgsql.
/// </summary>
public static class AltinnServiceDefaultsNpgsqlExtensions
{
    private static string DefaultConfigSectionName(string connectionName)
        => $"Altinn:Npgsql:{connectionName}";

    /// <summary>
    /// Adds a PostgreSQL data source with optional initialization to the host.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <param name="configureSettings">Optional settings configuration delegate.</param>
    /// <param name="configureDataSourceBuilder">Optional datasource builder configuration delegate.</param>
    /// <returns>A <see cref="INpgsqlDatabaseBuilder"/> for further configuration.</returns>
    public static INpgsqlDatabaseBuilder AddAltinnPostgresDataSource(
        this IHostApplicationBuilder builder,
        Action<NpgsqlSettings>? configureSettings = null,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder = null)
    {
        var serviceDescriptor = builder.GetAltinnServiceDescriptor();

        return AddAltinnPostgresDataSource(builder, $"{serviceDescriptor.Name}", configureSettings, configureDataSourceBuilder);
    }

    /// <summary>
    /// Adds a PostgreSQL data source with optional initialization to the host.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <param name="configureDataSourceBuilder">Optional datasource builder configuration delegate.</param>
    /// <returns>A <see cref="INpgsqlDatabaseBuilder"/> for further configuration.</returns>
    public static INpgsqlDatabaseBuilder AddAltinnPostgresDataSource(
        this IHostApplicationBuilder builder,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
        => AddAltinnPostgresDataSource(builder, configureSettings: null, configureDataSourceBuilder);

    /// <summary>
    /// Adds a PostgreSQL data source with optional initialization to the host.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/>.</param>
    /// <param name="connectionName">The connection name.</param>
    /// <param name="configureSettings">Optional settings configuration delegate.</param>
    /// <param name="configureDataSourceBuilder">Optional datasource builder configuration delegate.</param>
    /// <returns>A <see cref="INpgsqlDatabaseBuilder"/> for further configuration.</returns>
    public static INpgsqlDatabaseBuilder AddAltinnPostgresDataSource(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<NpgsqlSettings>? configureSettings = null,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder = null)
        => AddAltinnPostgresDataSource(builder, DefaultConfigSectionName(connectionName), configureSettings, connectionName, configureDataSourceBuilder: configureDataSourceBuilder);

    private static INpgsqlDatabaseBuilder AddAltinnPostgresDataSource(
        IHostApplicationBuilder builder,
        string configurationSectionName,
        Action<NpgsqlSettings>? configureSettings,
        string connectionName,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
    {
        Guard.IsNotNull(builder);

        if (builder.Services.Any(s => s.ServiceType == typeof(NpgsqlDatabaseHostedServiceMarker)))
        {
            // already registered
            return new NpgsqlDatabaseBuilder(builder.Services);
        }

        NpgsqlSettings settings = new();
        builder.Configuration.GetSection(configurationSectionName).Bind(settings);

        if (builder.Configuration.GetConnectionString($"{connectionName}_db") is string connectionString)
        {
            settings.ConnectionString = connectionString;
            settings.Migrate.ConnectionString = connectionString;
            settings.Seed.ConnectionString = connectionString;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_migrator") is string migratorConnectionString)
        {
            settings.Migrate.ConnectionString = migratorConnectionString;
            settings.Seed.ConnectionString = migratorConnectionString;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_seed") is string seedConnectionString)
        {
            settings.Seed.ConnectionString = seedConnectionString;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_server") is string serverConnectionString)
        {
            settings.Create.ConnectionString = serverConnectionString;
        }

        configureSettings?.Invoke(settings);

        builder.Services.AddSingleton<NpgsqlDatabaseHostedServiceMarker>();
        builder.Services.AddSingleton<IHostedService, NpgsqlDatabaseHostedService>();
        builder.RegisterNpgsqlServices(settings, configurationSectionName, connectionName, configureDataSourceBuilder);

        if (!settings.DisableHealthChecks)
        {
            builder.TryAddHealthCheck(new HealthCheckRegistration(
                "PostgreSql",
                sp => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(sp.GetRequiredService<NpgsqlDataSource>())),
                failureStatus: default,
                tags: default,
                timeout: default));
        }

        if (!settings.DisableTracing)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(traceProviderBuilder =>
                {
                    traceProviderBuilder.AddNpgsql();
                });
        }

        if (!settings.DisableMetrics)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(meterProviderBuilder =>
                {
                    double[] secondsBuckets = [0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10];

                    // https://github.com/npgsql/npgsql/blob/4c9921de2dfb48fb5a488787fc7422add3553f50/src/Npgsql/MetricsReporter.cs#L48
                    meterProviderBuilder
                        .AddMeter("Npgsql")
                        // Npgsql's histograms are in seconds, not milliseconds.
                        .AddView("db.client.commands.duration",
                            new ExplicitBucketHistogramConfiguration
                            {
                                Boundaries = secondsBuckets
                            })
                        .AddView("db.client.connections.create_time",
                            new ExplicitBucketHistogramConfiguration
                            {
                                Boundaries = secondsBuckets
                            });
                });
        }

        var dbBuilder = new NpgsqlDatabaseBuilder(builder.Services);

        if (builder.Environment.IsDevelopment() && settings.Create.Enabled)
        {
            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(connectionName, options =>
            {
                options.CreateDatabase = true;
                options.CreateDatabaseConnectionString = settings.Create.ConnectionString;
            });

            if (string.IsNullOrEmpty(settings.Create.DatabaseName))
            {
                ThrowHelper.ThrowArgumentException("DatabaseName must be provided when Create.Enabled is true.");
            }

            foreach (var (key, role) in settings.Create.Roles)
            {
                if (string.IsNullOrEmpty(role.Name))
                {
                    ThrowHelper.ThrowArgumentException($"Role name must be provided when Create.Enabled is true. Role: {key}");
                }

                dbBuilder.CreateRole(role.Name, role.Password);
            }

            dbBuilder.CreateDatabase(settings.Create.DatabaseName!, settings.Create.DatabaseOwner);
        }

        if (settings.Migrate.Enabled)
        {
            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(connectionName, options =>
            {
                options.MigrateDatabase = true;
                options.MigrationConnectionString = settings.Migrate.ConnectionString;
            });

            //var connectionStringBuilder = new NpgsqlConnectionStringBuilder(settings.Migrate.ConnectionString);
            var migratorRole = new NpgsqlConnectionStringBuilder(settings.Migrate.ConnectionString).Username!;
            var appRole = new NpgsqlConnectionStringBuilder(settings.ConnectionString).Username!;

            builder.Services.Configure<NpgsqlDatabaseMigrationOptions>(connectionName, options =>
            {
                options.AppRole = appRole;
                options.MigratorRole = migratorRole;
            });
        }

        if (builder.Environment.IsDevelopment() && settings.Seed.Enabled)
        {
            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(connectionName, options =>
            {
                options.SeedDatabase = true;
                options.SeedConnectionString = settings.Seed.ConnectionString;
            });
        }

        return dbBuilder;
    }

    private static void RegisterNpgsqlServices(
        this IHostApplicationBuilder builder,
        NpgsqlSettings settings,
        string configurationSectionName,
        string connectionName,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
    {
        var dataSourceLifetime = ServiceLifetime.Singleton;
        var connectionLifetime = ServiceLifetime.Transient;

        var services = builder.Services;

        services.AddOptions<JsonOptions>();

        services.TryAdd(
            new ServiceDescriptor(
                typeof(NpgsqlMultiHostDataSource),
                (sp) =>
                {
                    ValidateConnection();

                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(settings.ConnectionString);
                    dataSourceBuilder.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

                    var setup = sp.GetServices<IConfigureOptions<NpgsqlDataSourceBuilder>>();
                    var post = sp.GetServices<IPostConfigureOptions<NpgsqlDataSourceBuilder>>();
                    var validators = sp.GetServices<IValidateOptions<NpgsqlDataSourceBuilder>>();
                    var jsonOptionsMonitor = sp.GetRequiredService<IOptionsMonitor<JsonOptions>>();

                    ConfigureDataSourceBuilder(dataSourceBuilder, jsonOptionsMonitor.CurrentValue, setup, post, validators, configureDataSourceBuilder);
                    return dataSourceBuilder.BuildMultiHost();
                },
                dataSourceLifetime));

        services.TryAdd(
            new ServiceDescriptor(
                typeof(NpgsqlDataSource),
                (sp) => sp.GetRequiredService<NpgsqlMultiHostDataSource>(),
                dataSourceLifetime));

        services.TryAdd(
            new ServiceDescriptor(
                typeof(NpgsqlConnection),
                sp => sp.GetRequiredService<NpgsqlDataSource>().CreateConnection(),
                connectionLifetime));

        services.TryAdd(
            new ServiceDescriptor(
                typeof(DbDataSource),
                sp => sp.GetRequiredService<NpgsqlDataSource>(),
                dataSourceLifetime));

        services.TryAdd(
            new ServiceDescriptor(
                typeof(DbConnection),
                sp => sp.GetRequiredService<NpgsqlConnection>(),
                connectionLifetime));

        void ValidateConnection()
        {
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                ThrowHelper.ThrowInvalidOperationException(
                    $"ConnectionString is missing. It should be provided in 'ConnectionStrings:{connectionName}_db' or under the 'ConnectionString' key in '{configurationSectionName}' configuration section.");
            }
        }

        static void ConfigureDataSourceBuilder(
            NpgsqlDataSourceBuilder builder,
            JsonOptions jsonOptions,
            IEnumerable<IConfigureOptions<NpgsqlDataSourceBuilder>> setups,
            IEnumerable<IPostConfigureOptions<NpgsqlDataSourceBuilder>> post,
            IEnumerable<IValidateOptions<NpgsqlDataSourceBuilder>> validators,
            Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
        {
            builder.ConfigureJsonOptions(jsonOptions.SerializerOptions);

            foreach (var setup in setups)
            {
                setup.Configure(builder);
            }

            configureDataSourceBuilder?.Invoke(builder);

            foreach (var postConfigure in post)
            {
                postConfigure.PostConfigure(Options.Options.DefaultName, builder);
            }

            List<string>? failures = null;
            foreach (var validate in validators)
            {
                ValidateOptionsResult result = validate.Validate(Options.Options.DefaultName, builder);
                if (result is not null && result.Failed)
                {
                    failures ??= new List<string>();
                    failures.AddRange(result.Failures);
                }

                if (failures is { Count: > 0 })
                {
                    throw new OptionsValidationException(Options.Options.DefaultName, typeof(NpgsqlDataSourceBuilder), failures);
                }
            }
        }
    }

    private class NpgsqlDatabaseHostedServiceMarker
    {
    }
}
