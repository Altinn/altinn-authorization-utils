using Altinn.Authorization.ServiceDefaults.Npgsql.Tests.Utils;
using Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;
using Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public abstract partial class DatabaseTestsBase
{
    protected AppContext.Builder CreateBuilder()
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
                "Altinn:Npgsql:test:Seed": {
                    "Enabled": true,
                    "ConnectionString": "{{appConnectionString}}"
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
        var dbBuilder = hostAppBuilder.AddAltinnPostgresDataSource();

        return new(hostAppBuilder, dbBuilder);
    }

    protected sealed partial class AppContext
    {
        public sealed class Builder
        {
            private readonly HostApplicationBuilder _hostBuilder;
            private readonly INpgsqlDatabaseBuilder _dbBuilder;

            public Builder(
                HostApplicationBuilder hostBuilder, 
                INpgsqlDatabaseBuilder dbBuilder)
            {
                _hostBuilder = hostBuilder;
                _dbBuilder = dbBuilder;
            }

            public Builder ConfigureNpgsql(Action<NpgsqlDataSourceBuilder> configure)
            {
                _dbBuilder.Configure(configure);

                return this;
            }

            public Builder AddYuniqlMigrations(Action<YuniqlDatabaseMigratorOptions> configure)
            {
                _dbBuilder.AddYuniqlMigrations(configure);

                return this;
            }

            public Builder AddYuniqlMigrations(object? serviceKey, Action<YuniqlDatabaseMigratorOptions> configure)
            {
                _dbBuilder.AddYuniqlMigrations(serviceKey, configure);

                return this;
            }

            public Builder AddYuniqlMigrations(IEnumerable<string> scripts)
            {
                return AddYuniqlMigrations(YuniqlTestFileProvider.Create(scripts));
            }

            public Builder AddYuniqlMigrations(object? serviceKey, IEnumerable<string> scripts)
            {
                return AddYuniqlMigrations(serviceKey, YuniqlTestFileProvider.Create(scripts));
            }

            public Builder AddYuniqlMigrations(IFileProvider fs)
            {
                return AddYuniqlMigrations(opts =>
                {
                    opts.WorkspaceFileProvider = fs;
                    opts.Workspace = "/";
                    opts.MigrationsTable.Schema = "yuniql";
                    opts.MigrationsTable.Name = "migrations";
                });
            }

            public Builder AddYuniqlMigrations(object? serviceKey,IFileProvider fs)
            {
                return AddYuniqlMigrations(serviceKey, opts =>
                {
                    opts.WorkspaceFileProvider = fs;
                    opts.Workspace = "/";
                    opts.MigrationsTable.Schema = "yuniql";
                    opts.MigrationsTable.Name = "migrations";
                });
            }

            public Builder AddTestSeedData(IFileProvider fs)
            {
                _dbBuilder.SeedFromFileProvider(fs);

                return this;
            }

            public Builder AddTestSeedData(IEnumerable<KeyValuePair<string, string>> scripts)
            {
                var fs = new InMemoryFileProvider();

                foreach (var script in scripts)
                {
                    fs.Root.CreateFile(script.Key, script.Value);
                }

                return AddTestSeedData(fs);
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public TaskAwaiter<AppContext> GetAwaiter()
                => Build().GetAwaiter();

            private async Task<AppContext> Build()
            {
                var waitTime = TimeSpan.FromSeconds(10);
                if (Debugger.IsAttached)
                {
                    waitTime = TimeSpan.FromDays(1);
                }

                var app = _hostBuilder.Build();
                try
                {
                    await app.StartAsync().WaitAsync(waitTime);
                    var ctx = new AppContext(app);
                    app = null;
                    return ctx;
                }
                finally
                {
                    if (app is not null)
                    {
                        await app.StopAsync().WaitAsync(waitTime);
                        app.Dispose();
                    }
                }
            }
        }
    }
}
