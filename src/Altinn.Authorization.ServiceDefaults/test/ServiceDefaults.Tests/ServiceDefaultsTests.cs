using Altinn.Authorization.ServiceDefaults.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ServiceDefaultsTests
{
    private static readonly Microsoft.AspNetCore.HttpOverrides.IPNetwork TestNetwork 
        = new Microsoft.AspNetCore.HttpOverrides.IPNetwork(new IPAddress([10, 50, 0, 15]), 16);

    [Fact]
    public async Task NoClusterInfo()
    {
        await using var app = await CreateApp([]);

        var altinnClusterInfo = app.GetRequiredService<IOptionsMonitor<AltinnClusterInfo>>().CurrentValue;
        var forwardedHeadersOptions = app.GetRequiredService<IOptionsMonitor<ForwardedHeadersOptions>>().CurrentValue;
        Assert.NotNull(altinnClusterInfo);

        altinnClusterInfo.ClusterNetwork.Should().BeNull();
        forwardedHeadersOptions.KnownNetworks.Should().NotContain(TestNetwork);
    }

    [Fact]
    public async Task ValidCidrInClusterInfo()
    {
        await using var app = await CreateApp([
           KeyValuePair.Create("Altinn:ClusterInfo:ClusterNetwork", $"{TestNetwork.Prefix}/{TestNetwork.PrefixLength}"),
        ]);

        var altinnClusterInfo = app.GetRequiredService<IOptionsMonitor<AltinnClusterInfo>>().CurrentValue;
        var forwardedHeadersOptions = app.GetRequiredService<IOptionsMonitor<ForwardedHeadersOptions>>().CurrentValue;
        Assert.NotNull(altinnClusterInfo);

        altinnClusterInfo.ClusterNetwork.Should().BeEquivalentTo(IPNetworkUtils.From(TestNetwork));
        forwardedHeadersOptions.KnownNetworks.Should().ContainEquivalentOf(altinnClusterInfo.ClusterNetwork);
    }

    [Fact]
    public async Task InvalidCidrInClusterInfo()
    {
        Func<Task> act = async () =>
        {
            await using var app = await CreateApp([
                KeyValuePair.Create("Altinn:ClusterInfo:ClusterNetwork", "text"),
            ]);

            var _altinnClusterInfo = app.GetRequiredService<IOptionsMonitor<AltinnClusterInfo>>().CurrentValue;
        };

        await act.Should().ThrowAsync<FormatException>();
    }

    private async Task<AppContext> CreateApp(ImmutableArray<KeyValuePair<string, string>> config)
    {
        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(config);

        var hostAppBuilder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            ApplicationName = "test",
            EnvironmentName = "Development",
            DisableDefaults = true,
            //ContentRootPath = wwwDir.FullName,
            Configuration = configuration,
        });

        hostAppBuilder.AddAltinnServiceDefaults("test");

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
        , IServiceProvider
    {
        private readonly IHost _host;

        public AppContext(
            IHost host)
        {
            _host = host;
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

        object? IServiceProvider.GetService(Type serviceType)
            => _host.Services.GetService(serviceType);
    }
}
