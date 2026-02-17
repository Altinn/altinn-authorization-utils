using Testcontainers.PostgreSql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public sealed class DbFixture
    : IAsyncLifetime
{
    private PostgreSqlContainer _container
        = new PostgreSqlBuilder("ghcr.io/altinn/library/postgres:16.2-alpine")
            .WithUsername("superadmin_user")
            .WithPassword("superadmin_password")
            .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        
        await _container.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
    }

    public string ConnectionString =>
        _container.GetConnectionString();
}
