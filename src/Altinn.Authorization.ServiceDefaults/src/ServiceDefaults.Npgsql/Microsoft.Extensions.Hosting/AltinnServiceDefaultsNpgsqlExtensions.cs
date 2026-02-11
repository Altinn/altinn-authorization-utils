using Altinn.Authorization.ServiceDefaults.Npgsql;
using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;
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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Host builder extensions for Npgsql.
/// </summary>
[ExcludeFromCodeCoverage]
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

    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "This is an overload for public methods that return the interface")]
    private static INpgsqlDatabaseBuilder AddAltinnPostgresDataSource(
        IHostApplicationBuilder builder,
        string configurationSectionName,
        Action<NpgsqlSettings>? configureSettings,
        string connectionName,
        Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
    {
        Guard.IsNotNull(builder);

        var configuration = builder.Configuration.GetSection(configurationSectionName);
        if (builder.Services.Contains(Marker.ServiceDescriptor))
        {
            // already registered
            return new NpgsqlDatabaseBuilder(builder.Services, configuration);
        }

        builder.Services.Add(Marker.ServiceDescriptor);

        NpgsqlSettings settings = new();
        configuration.Bind(settings);

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_seed") is string connDbSeed)
        {
            settings.Seed.ConnectionString ??= connDbSeed;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_cluster") is string connDbCluster)
        {
            settings.Create.ClusterConnectionString ??= connDbCluster;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_init") is string connDbCreator)
        {
            settings.Create.DatabaseConnectionString ??= connDbCreator;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db_migrate") is string connDbMigrate)
        {
            settings.Migrate.ConnectionString ??= connDbMigrate;
            settings.Seed.ConnectionString ??= connDbMigrate;
        }

        if (builder.Configuration.GetConnectionString($"{connectionName}_db") is string connDb)
        {
            settings.ConnectionString ??= connDb;
            settings.Migrate.ConnectionString ??= connDb;
            settings.Seed.ConnectionString ??= connDb;
        }

        configureSettings?.Invoke(settings);

        builder.Services.AddSingleton<IHostedService, NpgsqlDatabaseHostedService>();
        builder.RegisterNpgsqlServices(settings, configurationSectionName, connectionName, configureDataSourceBuilder);

        if (!settings.DisableHealthChecks)
        {
            builder.TryAddHealthCheck(new HealthCheckRegistration(
                "PostgreSql",
                sp => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(sp.GetRequiredService<NpgsqlDataSource>()) { CommandText = NpgsqlConsts.HealthCheckCommandText }),
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

        var dbBuilder = new NpgsqlDatabaseBuilder(builder.Services, configuration);

        if (builder.Environment.IsDevelopment() && settings.Create.Enabled)
        {
            var dbName = settings.Create.DatabaseName;
            var clusterConnectionString = settings.Create.ClusterConnectionString;
            var initConnectionString = settings.Create.DatabaseConnectionString;

            if (string.IsNullOrEmpty(dbName))
            {
                ThrowHelper.ThrowArgumentException("DatabaseName must be provided when Create.Enabled is true.");
            }

            string? dbOwner = null, dbPassword = null;
            if (!string.IsNullOrEmpty(settings.Create.DatabaseOwner))
            {
                if (!settings.Create.Roles.TryGetValue(settings.Create.DatabaseOwner, out var ownerRole))
                {
                    ThrowHelper.ThrowArgumentException($"DatabaseOwner '{settings.Create.DatabaseOwner}' is not defined in Roles.");
                }

                dbOwner = ownerRole.Name;
                dbPassword = ownerRole.Password;
            }

            if (string.IsNullOrEmpty(initConnectionString))
            {
                var connBuilder = new NpgsqlConnectionStringBuilder(clusterConnectionString);
                connBuilder.Database = dbName;

                if (dbOwner is not null)
                {
                    if (dbPassword is null)
                    {
                        ThrowHelper.ThrowArgumentException($"Password must be provided for the database owner '{dbOwner}' when {nameof(settings.Create)}.{nameof(settings.Create.DatabaseConnectionString)} is not set.");
                    }

                    connBuilder.Username = dbOwner;
                    connBuilder.Password = dbPassword;
                }

                initConnectionString = connBuilder.ConnectionString;
            }

            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(options =>
            {
                options.CreateDatabase = true;
                options.CreateDatabaseClusterConnectionString = clusterConnectionString;
                options.CreateDatabaseInitConnectionString = initConnectionString;
            });

            foreach (var (key, schema) in settings.Create.Schemas)
            {
                if (string.IsNullOrWhiteSpace(schema.Name))
                {
                    ThrowHelper.ThrowArgumentException($"Schema name must be provided when Create.Enabled is true. Schema: {key}");
                }

                string? schemaOwner = null;
                if (!string.IsNullOrWhiteSpace(schema.Owner))
                {
                    if (!settings.Create.Roles.TryGetValue(schema.Owner, out var role))
                    {
                        ThrowHelper.ThrowArgumentException($"Schema owner '{schema.Owner}' is not defined in Roles. Schema: {key}");
                    }

                    schemaOwner = role.Name;
                }

                dbBuilder.CreateSchema(schema.Name, schemaOwner);
            }

            foreach (var (key, role) in settings.Create.Roles)
            {
                if (string.IsNullOrWhiteSpace(role.Name))
                {
                    ThrowHelper.ThrowArgumentException($"Role name must be provided when Create.Enabled is true. Role: {key}");
                }

                dbBuilder.CreateRole(role.Name, role.Password);
                dbBuilder.GrantDatabasePrivileges(databaseName: dbName, roleName: role.Name, role.Grants.Database.Privileges, role.Grants.Database.WithGrantOption);

                foreach (var (schemaId, schemaGrant) in role.Grants.Schemas)
                {
                    if (!settings.Create.Schemas.TryGetValue(schemaId, out var schema))
                    {
                        ThrowHelper.ThrowArgumentException($"Schema '{schemaId}' is not defined in Schemas. Role: {key}");
                    }

                    dbBuilder.GrantSchemaPrivileges(schema.Name!, role.Name, schemaGrant.Privileges, schemaGrant.WithGrantOption);
                }

                foreach (var (grantedRole, roleGrant) in role.Grants.Roles)
                {
                    if (!settings.Create.Roles.TryGetValue(grantedRole, out var grantedRoleSettings))
                    {
                        ThrowHelper.ThrowArgumentException($"Role '{grantedRole}' is not defined in Roles. Role: {key}");
                    }

                    if (roleGrant.Usage)
                    {
                        dbBuilder.GrantRoleToRole(role.Name, grantedRoleSettings.Name!);
                    }

                    foreach (var (schemaId, schemaGrant) in roleGrant.Schemas)
                    {
                        if (!settings.Create.Schemas.TryGetValue(schemaId, out var schema))
                        {
                            ThrowHelper.ThrowArgumentException($"Schema '{schemaId}' is not defined in Schemas. Role: {key}, GrantedTo: {grantedRole}");
                        }

                        dbBuilder.GrantDefaultTablePrivilegesInSchema(
                            creatorRoleName: grantedRoleSettings.Name!,
                            roleName: role.Name,
                            schemaName: schema.Name!,
                            schemaGrant.Tables.Privileges,
                            schemaGrant.Tables.WithGrantOption);

                        dbBuilder.GrantDefaultSequencePrivilegesInSchema(
                            creatorRoleName: grantedRoleSettings.Name!,
                            roleName: role.Name,
                            schemaName: schema.Name!,
                            schemaGrant.Sequences.Privileges,
                            schemaGrant.Sequences.WithGrantOption);
                    }
                }
            }

            dbBuilder.CreateDatabase(dbName, dbOwner);
        }

        if (settings.Migrate.Enabled)
        {
            var migrateConnectionString = settings.Migrate.ConnectionString;
            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(options =>
            {
                options.MigrateDatabase = true;
                options.MigrationConnectionString = migrateConnectionString;
            });

            var migratorRole = new NpgsqlConnectionStringBuilder(settings.Migrate.ConnectionString).Username!;
            var appRole = new NpgsqlConnectionStringBuilder(settings.ConnectionString).Username!;

            builder.Services.Configure<NpgsqlDatabaseMigrationOptions>(options =>
            {
                options.AppRole = appRole;
                options.MigratorRole = migratorRole;
            });
        }

        if (builder.Environment.IsDevelopment() && settings.Seed.Enabled)
        {
            var seedConnectionString = settings.Seed.ConnectionString;
            builder.Services.Configure<NpgsqlDatabaseHostedService.Options>(options =>
            {
                options.SeedDatabase = true;
                options.SeedConnectionString = seedConnectionString;
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

                    var configureDefaultTelemetry = sp.GetServices<IConfigureOptions<INpgsqlTelemetryOptions>>();
                    var postConfigureDefaultTelemetry = sp.GetServices<IPostConfigureOptions<INpgsqlTelemetryOptions>>();
                    var telemetryBuilder = AltinnNpgsqlTelemetry.CreateBuilder();

                    foreach (var configure in configureDefaultTelemetry)
                    {
                        configure.Configure(telemetryBuilder);
                    }

                    foreach (var postConfigure in postConfigureDefaultTelemetry)
                    {
                        postConfigure.PostConfigure(Options.Options.DefaultName, telemetryBuilder);
                    }

                    ConfigureDataSourceBuilder(dataSourceBuilder, telemetryBuilder.Build(), jsonOptionsMonitor.CurrentValue, setup, post, validators, configureDataSourceBuilder);
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
            AltinnNpgsqlTelemetry telemetry,
            JsonOptions jsonOptions,
            IEnumerable<IConfigureOptions<NpgsqlDataSourceBuilder>> setups,
            IEnumerable<IPostConfigureOptions<NpgsqlDataSourceBuilder>> post,
            IEnumerable<IValidateOptions<NpgsqlDataSourceBuilder>> validators,
            Action<NpgsqlDataSourceBuilder>? configureDataSourceBuilder)
        {
            builder.ConfigureJsonOptions(jsonOptions.SerializerOptions);
            builder.ConfigureTracing(o =>
            {
                o.EnableFirstResponseEvent(false);
                
                o.ConfigureCommandSpanNameProvider(telemetry.GetCommandSpanName);
                o.ConfigureCommandFilter(telemetry.ShouldTraceCommand);

                o.ConfigureBatchSpanNameProvider(telemetry.GetBatchSpanName);
                o.ConfigureBatchFilter(telemetry.ShouldTraceBatch);

                o.ConfigureCommandEnrichmentCallback(telemetry.EnrichCommand);
                o.ConfigureBatchEnrichmentCallback(telemetry.EnrichBatch);
            });

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

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Marker, Marker>();
    }
}
