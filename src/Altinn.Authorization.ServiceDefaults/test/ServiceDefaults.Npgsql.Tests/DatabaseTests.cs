using Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class DatabaseTests
    : IClassFixture<DbFixture>
{
    private static int _counter = 0;

    private readonly DbFixture _fixture;

    public DatabaseTests(
        DbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestMigration()
    {
        await using var ctx = await RunMigrations([
            /*strpsql*/"CREATE TABLE app.test (id BIGINT PRIMARY KEY NOT NULL);",
            /*strpsql*/"INSERT INTO app.test (id) VALUES (1);",
        ]);

        var id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(1);

        var modified = await ctx.ExecuteNonQuery(/*strpsql*/"INSERT INTO app.test (id) VALUES (2), (3)");
        modified.Should().Be(2);

        id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(3);

        modified = await ctx.ExecuteNonQuery(/*strpsql*/"DELETE FROM app.test WHERE id = 3");
        modified.Should().Be(1);

        id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(2);
    }

    [Fact]
    public async Task TestSequencedTable()
    {
        await using var ctx = await RunMigrations([
            /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value BIGINT NOT NULL)",
        ]);

        var id1 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (1) RETURNING id");
        var id2 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (2) RETURNING id");

        var value1 = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

        value1.Should().Be(1);
        value2.Should().Be(2);
    }

    [Fact]
    public async Task TestTableWithCustomType()
    {
        await using var ctx = await RunMigrations([
            /*strpsql*/"CREATE TYPE app.test_enum AS ENUM ('one', 'two', 'three')",
            /*strpsql*/"CREATE TYPE app.test_composite AS (first app.test_enum, second app.test_enum)",
            /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value app.test_composite NOT NULL)",
        ], cfg =>
        {
            cfg.MapEnum<TestEnum>("app.test_enum");
            cfg.MapComposite<TestComposite>("app.test_composite");
        });

        var id1 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('one'::app.test_enum, 'one'::app.test_enum)::app.test_composite) RETURNING id");
        var id2 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('two'::app.test_enum, 'three'::app.test_enum)::app.test_composite) RETURNING id");

        var value1 = await ctx.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

        value1.First.Should().Be(TestEnum.One);
        value1.Second.Should().Be(TestEnum.One);
        value2.First.Should().Be(TestEnum.Two);
        value2.Second.Should().Be(TestEnum.Three);
    }

    [Fact]
    public async Task TestFunction()
    {
        await using var ctx = await RunMigrations([
            /*strpsql*/"""
            CREATE FUNCTION app.test_function (value BIGINT)
            RETURNS BIGINT
            AS $$
            BEGIN
                RETURN value + 1;
            END;
            $$ LANGUAGE plpgsql;
            """
        ]);

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    [Fact]
    public async Task TestWithEmbeddedFileProvider()
    {
        var fs = new ManifestEmbeddedFileProvider(typeof(DatabaseTests).Assembly, "TestMigrations/Test1");
        await using var ctx = await RunMigrations(fs);

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    private Task<AppContext> RunMigrations(
        IEnumerable<string> scripts,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        var fs = new InMemoryFileProvider();
        var testCount = Interlocked.Increment(ref _counter);

        var dir = fs.Root;
        var version = 0;
        foreach (var script in scripts)
        {
            var versionDir = dir.CreateSubdirectory($"v{version++}.00");
            versionDir.CreateFile("00-script.sql", script);
        }

        return RunMigrations(fs, configure);
    }

    private async Task<AppContext> RunMigrations(
        IFileProvider fs,
        Action<NpgsqlDataSourceBuilder>? configure = null)
    {
        const string OWNER_USER = "owner_user";
        const string OWNER_PASSWORD = "owner_password";

        const string MIGRATOR_USER = "migrator_user";
        const string MIGRATOR_PASSWORD = "migrator_password";

        const string APP_USER = "app_user";
        const string APP_PASSWORD = "app_password";

        var testCount = Interlocked.Increment(ref _counter);

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
        configuration.AddJsonConfiguration(
            $$"""
            {
                "Altinn:Npgsql:test:ConnectionString": "{{appConnectionString}}",
                "Altinn:Npgsql:test:Migrate": {
                    "Enabled": true,
                    "ConnectionString": "{{migratorConnectionString}}"
                },
                "Altinn:Npgsql:test:Create": {
                    "Enabled": true,
                    "ClusterConnectionString": "{{clusterConnectionString}}",
                    "DatabaseConnectionString": "{{creatorDatabaseConnectionString}}",
                    "DatabaseName": "{{dbName}}",
                    "DatabaseOwner": "owner",
                    "Schemas": {
                        "yuniql": {
                            "Name": "yuniql",
                            "Owner": "migrator"
                        },
                        "app": {
                            "Name": "app",
                            "Owner": "migrator"
                        }
                    },
                    "Roles": {
                        "owner": {
                            "Name": "{{OWNER_USER}}",
                            "Password": "{{OWNER_PASSWORD}}",
                            "Grants": {
                                "Roles": {
                                    "migrator": {
                                        "Usage": true
                                    },
                                    "app": {
                                        "Usage": true
                                    }
                                }
                            }
                        },
                        "migrator": {
                            "Name": "{{MIGRATOR_USER}}",
                            "Password": "{{MIGRATOR_PASSWORD}}",
                            "Grants": {
                                "Database": {
                                    "Privileges": "Connect",
                                    "WithGrantOption": true
                                }
                            }
                        },
                        "app": {
                            "Name": "{{APP_USER}}",
                            "Password": "{{APP_PASSWORD}}",
                            "Grants": {
                                "Database": {
                                    "Privileges": "Connect"
                                },
                                "Schemas": {
                                    "app": {
                                        "Privileges": "Usage"
                                    }
                                },
                                "Roles": {
                                    "migrator": {
                                        "Schemas": {
                                            "app": {
                                                "Tables": {
                                                    "Privileges": "Usage"
                                                },
                                                "Sequences": {
                                                    "Privileges": "Usage"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            """
        );

        var hostAppBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            ApplicationName = "test",
            EnvironmentName = "Development",
            DisableDefaults = true,
            //ContentRootPath = wwwDir.FullName,
            Configuration = configuration,
        });

        hostAppBuilder.AddAltinnServiceDefaults("test");

        hostAppBuilder
            .AddAltinnPostgresDataSource(configureDataSourceBuilder: configure)
            .AddYuniqlMigrations(opts =>
            {
                opts.WorkspaceFileProvider = fs;
                opts.Workspace = "/";
                opts.MigrationsTable.Schema = "yuniql";
                opts.MigrationsTable.Name = "migrations";
            });

        var app = hostAppBuilder.Build();
        try
        {
            var waitTime = TimeSpan.FromSeconds(10);
            if (Debugger.IsAttached)
            {
                waitTime = TimeSpan.FromDays(1);
            }

            await app.StartAsync().WaitAsync(waitTime);
            var ctx = new AppContext(app);
            app = null;
            return ctx;
        }
        finally
        {
            if (app is not null)
            {
                await app.StopAsync();
                app.Dispose();
            }
        }
    }

    private class AppContext
        : IAsyncDisposable
    {
        private readonly IHost _host;

        public AppContext(
            IHost host)
        {
            _host = host;
        }

        public NpgsqlDataSource DataSource =>
            _host.Services.GetRequiredService<NpgsqlDataSource>();

        public async Task<T> ExecuteScalar<T>(string sql, params NpgsqlParameter[] parameters)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            var hasResult = await reader.ReadAsync();

            hasResult.Should().BeTrue("ExecuteScalar expects a result");
            return await reader.GetFieldValueAsync<T>(0);
        }

        public async Task<int> ExecuteNonQuery(string sql)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            return await cmd.ExecuteNonQueryAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();

            if (_host is IAsyncDisposable h)
            {
                await h.DisposeAsync();
            }

            _host.Dispose();
        }
    }

    private enum TestEnum
    {
        One,
        Two,
        Three,
    }

    private record TestComposite
    {
        public TestEnum First { get; init; }
        public TestEnum Second { get; init; }
    }
}
