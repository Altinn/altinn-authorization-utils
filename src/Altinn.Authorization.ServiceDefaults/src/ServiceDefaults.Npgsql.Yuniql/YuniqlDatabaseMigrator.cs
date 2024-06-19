using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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

        using var workspace = ResolveWorkspace(options);

        // Yuniql uses a lot of static state, so we need to ensure only a single migration service is running at a time
        lock (_lock)
        {
            // setup config
            var config = Configuration.Instance;
            config.Workspace = workspace.Path;
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

    private Workspace ResolveWorkspace(YuniqlDatabaseMigratorOptions options)
    {
        if (options.WorkspaceFileProvider is null)
        {
            return new Workspace(options.Workspace!, isTemp: false);
        }

        var provider = options.WorkspaceFileProvider;
        var relPath = options.Workspace ?? "";
        var fileInfo = provider.GetFileInfo(relPath);
        if ((!fileInfo.Exists || !fileInfo.IsDirectory)
            // Some file-providers does not return file-info for directories
            && !provider.GetDirectoryContents(relPath).Exists)
        {
            throw new DirectoryNotFoundException("Workspace path does not exist or is not a directory");
        }

        if (fileInfo.Exists && fileInfo.PhysicalPath is not null)
        {
            return new Workspace(fileInfo.PhysicalPath, isTemp: false);
        }

        var tempDir = CreateTempDir();
        try
        {
            WriteContents(provider, relPath, tempDir);
            EnsureYuniqlDirs(tempDir);
            var ret = new Workspace(tempDir.FullName, isTemp: true);
            tempDir = null;
            return ret;
        }
        finally
        {
            tempDir?.Delete(recursive: true);
        }
    }

    private static void WriteContents(IFileProvider provider, string path, DirectoryInfo target)
    {
        var contents = provider.GetDirectoryContents(path);
        if (!contents.Exists)
        {
            return;
        }

        foreach (var entry in contents)
        {
            if (entry.IsDirectory)
            {
                var subPath = Path.Combine(path, entry.Name);
                var subTarget = target.CreateSubdirectory(entry.Name);
                WriteContents(provider, subPath, subTarget);
            }
            else
            {
                var file = new FileInfo(Path.Combine(target.FullName, entry.Name));
                using var readStream = entry.CreateReadStream();
                using var writeStream = file.Create();
                readStream.CopyTo(writeStream);
            }
        }
    }

    private static void EnsureYuniqlDirs(DirectoryInfo target)
    {
        target.CreateSubdirectory("_init");
        target.CreateSubdirectory("_pre");
        target.CreateSubdirectory("_post");
        target.CreateSubdirectory("_draft");
        target.CreateSubdirectory("_erase");
    }

    private static DirectoryInfo CreateTempDir()
    {
        while (true)
        {
            var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (Path.Exists(dir))
            {
                continue;
            }

            return Directory.CreateDirectory(dir);
        }
    }

    private sealed class Workspace
        : IDisposable
    {
        private readonly bool _isTemp;

        public string Path { get; }

        public Workspace(string path, bool isTemp)
        {
            Path = path;
            _isTemp = isTemp;
        }

        public void Dispose()
        {
            if (_isTemp)
            {
                Directory.Delete(Path, recursive: true);
            }
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
