using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Altinn.Authorization.TestUtils.AspNetCore;

#if !NET9_0_OR_GREATER
using Lock=System.Object;
#endif

/// <summary>
/// Provides an HTTP client for integration testing ASP.NET Core applications using an in-memory test server. Enables
/// sending HTTP requests to a test host and managing its lifecycle within test scenarios.
/// </summary>
/// <remarks>TestClient extends HttpClient and manages the underlying test server and host, allowing for
/// end-to-end testing of web applications without external dependencies. Instances should be disposed asynchronously
/// using DisposeAsync to ensure proper cleanup of resources. This class is intended for use in automated tests and is
/// not suitable for production environments.</remarks>
public class TestClient
    : HttpClient
    , IAsyncDisposable
    , IDisposable
{
    /// <summary>
    /// Creates and starts a test web host using the specified configuration actions, and returns a client for
    /// interacting with the test server.
    /// </summary>
    /// <remarks>The returned <see cref="TestClient"/> uses a default timeout of 200 seconds and is configured
    /// with the test server's base address. The host is started asynchronously and will honor the current test
    /// context's cancellation token.</remarks>
    /// <param name="configureHost">An action that configures the <see cref="IWebHostBuilder"/> for the test host. Use this to set up services,
    /// middleware, and other host-level options.</param>
    /// <param name="configureApplication">An optional action that configures the application's request pipeline. Receives the <see
    /// cref="WebHostBuilderContext"/> and <see cref="IApplicationBuilder"/> for further customization. If <see
    /// langword="null"/>, no additional application configuration is applied.</param>
    /// <returns>A <see cref="TestClient"/> instance connected to the started test server, ready to send requests.</returns>
    public static async Task<TestClient> Create(
        Action<IWebHostBuilder> configureHost,
        Action<WebHostBuilderContext, IApplicationBuilder>? configureApplication = null)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var host = new HostBuilder()
            .ConfigureWebHost(builder =>
            {
                builder.UseTestServer();
                builder.Configure((ctx, app) => 
                {
                    configureApplication?.Invoke(ctx, app);
                });
                configureHost(builder);
            })
            .Build();

        await host.StartAsync(cancellationToken);
        var server = host.GetTestServer();
        var handler = server.CreateHandler();
        return new TestClient(handler, host)
        {
            BaseAddress = server.BaseAddress,
            Timeout = TimeSpan.FromSeconds(200),
        };
    }

    /// <summary>
    /// Creates a test client configured to host the specified controller type in an in-memory ASP.NET Core application
    /// for unit testing.
    /// </summary>
    /// <remarks>This method is intended for use in unit tests where a controller needs to be hosted in
    /// isolation. The returned client can be used to send HTTP requests to the controller endpoints without starting a
    /// real server. All configuration delegates are optional; if omitted, default settings are used.</remarks>
    /// <typeparam name="TController">The controller type to be hosted by the test client. Must be a non-abstract class.</typeparam>
    /// <param name="configureMvc">An optional delegate to further configure MVC services during application setup. Can be used to add filters,
    /// formatters, or other MVC options.</param>
    /// <param name="configureHost">An optional delegate to configure the web host builder before the application is built. Allows customization of
    /// host-level settings such as environment or configuration sources.</param>
    /// <param name="configureApplication">An optional delegate to configure the application's middleware pipeline. Receives the host context and
    /// application builder for advanced customization.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains a test client instance configured with
    /// the specified controller and customizations.</returns>
    public static Task<TestClient> CreateControllerClient<TController>(
        Action<IMvcBuilder>? configureMvc = null,
        Action<IWebHostBuilder>? configureHost = null,
        Action<WebHostBuilderContext, IApplicationBuilder>? configureApplication = null)
        where TController : class
    {
        return Create(
            builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var mvcBuilder = services.AddMvc();
                    var partManager = mvcBuilder.PartManager;

                    TypeInfo type = typeof(TController).GetTypeInfo();
                    partManager.ApplicationParts.Add(new TestControllerApplicationPart(type));
                    partManager.FeatureProviders.Add(TestControllerApplicationFeatureProvider.Instance);

                    configureMvc?.Invoke(mvcBuilder);
                });

                configureHost?.Invoke(builder);
            },
            (ctx, app) =>
            {
                app.UseRouting();

                configureApplication?.Invoke(ctx, app);

                app.UseEndpoints(builder => builder.MapControllers());
            });
    }

    private readonly IHost _host;
    private readonly Lock _lock = new();
    private Task? _dispose;

    private TestClient(HttpMessageHandler handler, IHost host)
        : base(handler, disposeHandler: false)
    {
        _host = host;
    }

    void IDisposable.Dispose()
    {
        ThrowHelper.ThrowInvalidOperationException("Use DisposeAsync instead.");
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        lock (_lock)
        {
            _dispose ??= DisposeAsyncCore(CancellationToken.None);
            return new ValueTask(_dispose);
        }

        async Task DisposeAsyncCore(CancellationToken cancellationToken)
        {
            Dispose(true);
            await _host.StopAsync(cancellationToken);
            if (_host is IAsyncDisposable h)
            {
                await h.DisposeAsync();
            }
            else
            {
                _host.Dispose();
            }
        }
    }
}
