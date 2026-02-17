using Altinn.Authorization.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public abstract partial class DatabaseTestsBase
{
    protected sealed partial class AppContext
        : IAsyncDisposable
    {
        private readonly IHost _host;
        private readonly DatabaseContext _db;

        public AppContext(
            IHost host)
        {
            _host = host;
            _db = new(host);
        }

        public DatabaseContext Database => _db;

        public IReadOnlyList<Activity> Activities
            => _host.Services.GetRequiredService<ActivityCollector>().Snapshot();

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

    protected sealed class DatabaseContext
    {
        private readonly IHost _host;

        public DatabaseContext(
            IHost host)
        {
            _host = host;
        }

        public NpgsqlDataSource DataSource =>
            _host.Services.GetRequiredService<NpgsqlDataSource>();

        public string MigratorRole =>
            _host.Services.GetRequiredService<IOptionsMonitor<Migration.NpgsqlDatabaseMigrationOptions>>()
                .CurrentValue.MigratorRole!;

        public string AppRole =>
            _host.Services.GetRequiredService<IOptionsMonitor<Migration.NpgsqlDatabaseMigrationOptions>>()
                .CurrentValue.AppRole!;

        public async Task<NpgsqlDataReader> ExecuteReader(string sql, params IEnumerable<NpgsqlParameter> parameters)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            return await cmd.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        }

        public async Task<T> ExecuteScalar<T>(string sql, params IEnumerable<NpgsqlParameter> parameters)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            var hasResult = await reader.ReadAsync(TestContext.Current.CancellationToken);

            hasResult.ShouldBeTrue("ExecuteScalar expects a result");
            return await reader.GetFieldValueAsync<T>(0);
        }

        public async Task<int> ExecuteNonQuery(string sql)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            return await cmd.ExecuteNonQueryAsync(TestContext.Current.CancellationToken);
        }
    }
}
