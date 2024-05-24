using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql.Tests;

public class YuniqlTests
    : IClassFixture<DbFixture>
{
    private static int _counter = 0;

    private readonly DbFixture _fixture;

    public YuniqlTests(
        DbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestMigration()
    {
        await RunMigrations([
            "CREATE TABLE app.test (id BIGINT PRIMARY KEY);",
            "INSERT INTO app.test (id) VALUES (1);",
        ]);
    }

    private async Task RunMigrations(
        IEnumerable<string> scripts)
    {
        const string OWNER_USER = "owner_user";
        const string OWNER_PASSWORD = "owner_password";

        const string MIGRATOR_USER = "migrator_user";
        const string MIGRATOR_PASSWORD = "migrator_password";

        const string APP_USER = "app_user";
        const string APP_PASSWORD = "app_password";

        await using var tempDir = TempDirectory.Create();
        var testCount = Interlocked.Increment(ref _counter);

        var wwwDir = tempDir.DirectoryInfo.CreateSubdirectory("www");
        var dbDir = wwwDir.CreateSubdirectory("db");

        dbDir.CreateSubdirectory("_init");
        dbDir.CreateSubdirectory("_pre");
        dbDir.CreateSubdirectory("_post");
        dbDir.CreateSubdirectory("_draft");
        dbDir.CreateSubdirectory("_erase");

        var version = 0;
        foreach (var script in scripts)
        {
            var versionDir = dbDir.CreateSubdirectory($"v{version++}.00");
            await File.WriteAllTextAsync(
                Path.Combine(versionDir.FullName, "00-script.sql"),
                script);
        }

        var builder = new NpgsqlConnectionStringBuilder(_fixture.ConnectionString);
        var clusterConnectionString = builder.ConnectionString;

        var dbName = $"test_{testCount}";
        builder.Database = dbName;
        builder.Username = OWNER_USER;
        builder.Password = OWNER_PASSWORD;
        var creatorDatabaseConnectionString = builder.ConnectionString;


        builder.Username = MIGRATOR_USER;
        builder.Password = MIGRATOR_PASSWORD;
        var migratorConnectionString = builder.ConnectionString;

        builder.Username = APP_USER;
        builder.Password = APP_PASSWORD;
        var appConnectionString = builder.ConnectionString;

        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection([
            new("Altinn:Npgsql:test:Create:Enabled", "true"),
            new("Altinn:Npgsql:test:Create:DatabaseName", dbName),
            new("Altinn:Npgsql:test:Create:DatabaseOwner", "owner"),
            new("Altinn:Npgsql:test:Create:Schemas:public:Name", "public"),
            new("Altinn:Npgsql:test:Create:Schemas:yuniql:Name", "yuniql"),
            new("Altinn:Npgsql:test:Create:Schemas:yuniql:Owner", "migrator"),
            new("Altinn:Npgsql:test:Create:Schemas:app:Name", "app"),
            new("Altinn:Npgsql:test:Create:Schemas:app:Owner", "migrator"),
            new("Altinn:Npgsql:test:Create:Roles:owner:Name", OWNER_USER),
            new("Altinn:Npgsql:test:Create:Roles:owner:Password", OWNER_PASSWORD),
            new("Altinn:Npgsql:test:Create:Roles:owner:Grants:Roles:migrator", "true"),
            new("Altinn:Npgsql:test:Create:Roles:owner:Grants:Roles:app", "true"),
            new("Altinn:Npgsql:test:Create:Roles:migrator:Name", MIGRATOR_USER),
            new("Altinn:Npgsql:test:Create:Roles:migrator:Password", MIGRATOR_PASSWORD),
            new("Altinn:Npgsql:test:Create:Roles:migrator:Grants:Database:Privileges", "All"),
            new("Altinn:Npgsql:test:Create:Roles:migrator:Grants:Database:WithGrantOption", "true"),
            new("Altinn:Npgsql:test:Create:Roles:app:Name", APP_USER),
            new("Altinn:Npgsql:test:Create:Roles:app:Password", APP_PASSWORD),
            new("Altinn:Npgsql:test:Create:Roles:app:Grants:Database:Privileges", "Connect"),
            //new("Altinn:Npgsql:test:Create:Grants:migrator:Role", "migrator"),
            //new("Altinn:Npgsql:test:Create:Grants:migrator:Kinds", "Table,Sequence,Schema"),
            //new("Altinn:Npgsql:test:Create:Grants:migrator:Schema", "public"),
            //new("Altinn:Npgsql:test:Create:Grants:migrator:WithGrantOptions", "true"),
            new("Altinn:Npgsql:test:Create:ClusterConnectionString", clusterConnectionString),
            new("Altinn:Npgsql:test:Create:DatabaseConnectionString", creatorDatabaseConnectionString),
            new("Altinn:Npgsql:test:Migrate:Enabled", "true"),
            new("Altinn:Npgsql:test:Migrate:ConnectionString", migratorConnectionString),
            new("Altinn:Npgsql:test:ConnectionString", appConnectionString),
        ]);

        var hostAppBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            ApplicationName = "test",
            EnvironmentName = "Development",
            DisableDefaults = true,
            ContentRootPath = wwwDir.FullName,
            Configuration = configuration,
        });

        hostAppBuilder.AddAltinnServiceDefaults("test");
        
        hostAppBuilder
            .AddAltinnPostgresDataSource()
            .AddYuniqlMigrations(opts =>
            {
                opts.Workspace = dbDir.FullName;
                opts.MigrationsTable.Schema = "yuniql";
                opts.MigrationsTable.Name = "migrations";
            });

        var app = hostAppBuilder.Build();
        await app.StartAsync();//.WaitAsync(TimeSpan.FromSeconds(10));
        await app.StopAsync();//.WaitAsync(TimeSpan.FromSeconds(10));
    }
}
