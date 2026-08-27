using Altinn.Authorization.TestUtils.AspNetCore.Authentication;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace Altinn.Authorization.TestUtils.AspNetCore;


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
    /// <param name="configureBuilder">An optional action that configures the <see cref="WebApplicationBuilder"/> for the test host. Use this to set up services,
    /// middleware, host options, and web host options.</param>
    /// <param name="configureApplication">An optional action that configures the application's request pipeline. If <see langword="null"/>, no additional
    /// application configuration is applied.</param>
    /// <param name="configureTestServer">An optional action that configures the test server options.</param>
    /// <returns>A <see cref="TestClient"/> instance connected to the started test server, ready to send requests.</returns>
    public static async Task<TestClient> Create(
        Action<WebApplicationBuilder>? configureBuilder = null,
        Action<WebApplication>? configureApplication = null,
        Action<TestServerOptions>? configureTestServer = null)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var builder = WebApplication.CreateSlimBuilder();

        configureBuilder?.Invoke(builder);
        builder.WebHost.UseTestServer(options =>
        {
            configureTestServer?.Invoke(options);
        });

        var app = builder.Build();
        configureApplication?.Invoke(app);

        await app.StartAsync(cancellationToken);

        var server = app.GetTestServer();
        var handler = server.CreateHandler();
        var userHandler = new UserMessageHandler(handler);

        return new TestClient(userHandler, app)
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
    /// <param name="configureBuilder">An optional delegate to configure the application builder before the application is built. Allows customization of
    /// services, host settings, web host settings, or configuration sources.</param>
    /// <param name="configureApplication">An optional delegate to configure the application's middleware pipeline.</param>
    /// <param name="configureTestServer">An optional action that configures the test server options.</param>
    /// <returns>A task that represents the asynchronous operation. The result contains a test client instance configured with
    /// the specified controller and customizations.</returns>
    public static Task<TestClient> CreateControllerClient<TController>(
        Action<IMvcBuilder>? configureMvc = null,
        Action<WebApplicationBuilder>? configureBuilder = null,
        Action<WebApplication>? configureApplication = null,
        Action<TestServerOptions>? configureTestServer = null)
        where TController : class
    {
        return Create(
            builder =>
            {
                var mvcBuilder = builder.Services.AddMvc()
                    .AddTestController<TController>();

                builder.Services.AddAuthentication("TestClient")
                    .AddTestAuthentication("TestClient");

                configureMvc?.Invoke(mvcBuilder);

                configureBuilder?.Invoke(builder);
            },
            app =>
            {
                app.UseAuthentication();

                app.UseRouting();
                app.UseAuthorization();

                configureApplication?.Invoke(app);

                app.MapControllers();
            },
            configureTestServer);
    }

    private readonly IHost _host;
    private readonly UserMessageHandler _handler;
    private readonly Lock _lock = new();

    private Task? _dispose;

    private TestClient(UserMessageHandler handler, IHost host)
        : base(handler, disposeHandler: false)
    {
        _handler = handler;
        _host = host;
    }

    /// <summary>
    /// Gets or sets the security principal representing the authenticated user associated with the current context.
    /// </summary>
    public ClaimsPrincipal? User
    {
        get => _handler.User;
        set => _handler.User = value;
    }

    /// <summary>
    /// Gets the application's service provider for resolving dependencies and accessing registered services.
    /// </summary>
    public IServiceProvider Services => _host.Services;

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

    private sealed class UserMessageHandler(HttpMessageHandler innerHandler)
        : DelegatingHandler(innerHandler)
    {
        /// <summary>
        /// Gets or sets the security principal representing the authenticated user associated with the current context.
        /// </summary>
        public ClaimsPrincipal? User { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (User is not null)
            {
                //var toSerialize = User.Identities.Select(i => i.Claims.Select())
                request.Headers.Add(
                    "X-TestClient-User",
                    Json.SerializeToString(JsonClaimsPrincipal.FromPrincipal(User)));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
