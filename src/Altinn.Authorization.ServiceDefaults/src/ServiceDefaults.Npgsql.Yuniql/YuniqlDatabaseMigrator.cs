using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Yuniql.Core;
using Yuniql.Extensibility;
using Yuniql.PostgreSql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

internal partial class YuniqlDatabaseMigrator
    : INpgsqlDatabaseMigrator
{
    private static readonly string TOOL_NAME = "Altinn-Npgsql-Yuniql";
    private static readonly string TOOL_VERSION = typeof(YuniqlDatabaseMigrator).Assembly.GetName().Version?.ToString() ?? "";

    private static readonly object _lock = new();

    private readonly IOptionsMonitor<YuniqlDatabaseMigratorOptions> _options;
    private readonly ILogger<YuniqlDatabaseMigrator> _logger;

    public YuniqlDatabaseMigrator(
        IOptionsMonitor<YuniqlDatabaseMigratorOptions> options,
        ILogger<YuniqlDatabaseMigrator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task MigrateDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        var traceService = scopedServices.GetRequiredService<ITraceService>();

        return Task.Factory.StartNew(
            () => MigrateDatabaseSync(connectionProvider.ConnectionString, traceService),
            cancellationToken,
            TaskCreationOptions.LongRunning | TaskCreationOptions.RunContinuationsAsynchronously,
            TaskScheduler.Default);
    }

    private void MigrateDatabaseSync(string connectionString, ITraceService traceService)
    {
        var options = _options.CurrentValue;

        // Yuniql uses a lot of static state, so we need to ensure only a single migration service is running at a time
        lock (_lock)
        {
            // setup config
            var config = Configuration.Instance;
            config.Workspace = options.Workspace;
            config.IsDebug = false;
            config.Platform = SUPPORTED_DATABASES.POSTGRESQL;
            config.ConnectionString = connectionString;
            config.CommandTimeout = 600;
            config.TargetVersion = null;
            config.IsAutoCreateDatabase = false;
            config.Tokens = [.. options.Tokens];
            config.BulkSeparator = ",";
            config.BulkBatchSize = 0;
            config.Environment = options.Environment;
            config.MetaSchemaName = options.MigrationsTable.Schema;
            config.MetaTableName = options.MigrationsTable.Name;
            config.TransactionMode = "session";
            config.IsContinueAfterFailure = null;
            config.IsRequiredClearedDraft = false;
            config.IsForced = false;
            config.IsVerifyOnly = false;
            config.AppliedByTool = TOOL_NAME;
            config.AppliedByToolVersion = TOOL_VERSION;
            config.IsInitialized = false;

            var dataService = new PostgreSqlDataService(traceService);
            var bulkImportService = new PostgreSqlBulkImportService(traceService);
            var directoryService = new DirectoryService(traceService);
            var fileService = new FileService();
            var workspaceService = new WorkspaceService(traceService, directoryService, fileService);
            var tokenReplacementService = new TokenReplacementService(traceService);
            var metadataService = new MetadataService(dataService, traceService, tokenReplacementService);
            var configurationService = new ConfigurationService(new EnvironmentService(), workspaceService, traceService);
            var migratorService = new MigrationService(
                workspaceService,
                dataService,
                bulkImportService,
                metadataService,
                tokenReplacementService,
                directoryService,
                fileService,
                traceService,
                configurationService);

            using var activity = YuniqlActivityProvider.StartActivity(ActivityKind.Internal, "migrate database", [
                new("yuniql.environment", options.Environment),
            ]);

            Log.RunningYuniqlMigrations(_logger);
            migratorService.Run();
            Log.YuniqlMigrationsComplete(_logger);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Running Yuniql migrations.")]
        public static partial void RunningYuniqlMigrations(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Yuniql migrations complete.")]
        public static partial void YuniqlMigrationsComplete(ILogger logger);
    }
}
