using Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class PlatformAccessTokenStartupValidatorTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task HandlerValidationDefaultsToEnabledAndCanBeDisabled(bool? validateOnStart, bool expectedValidation)
    {
        var builder = Host.CreateApplicationBuilder();
        var calls = new Calls();
        builder.Services.AddSingleton(calls);
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>(ServiceLifetime.Scoped);
        var httpClient = builder.Services.AddHttpClient("api")
            .ConfigureBaseAddress("https://example.com/api/")
            .ConfigurePrimaryHttpMessageHandler(() => new NeverSendHandler());
        if (validateOnStart is { } validate)
        {
            httpClient.AddPlatformAccessTokenHandler(validateOnStart: validate);
        }
        else
        {
            httpClient.AddPlatformAccessTokenHandler();
        }

        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
        if (expectedValidation)
        {
            calls.RequestUris.ShouldBe([new Uri("https://example.com/api/")]);
            calls.Disposed.ShouldBe(1);
        }
        else
        {
            calls.RequestUris.ShouldBeEmpty();
        }

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ProviderRegistrationAloneDoesNotValidate()
    {
        var builder = Host.CreateApplicationBuilder();
        var calls = new Calls();
        builder.Services.AddSingleton(calls);
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>();
        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        calls.RequestUris.ShouldBeEmpty();
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidatesEachClientOnceBeforeHostedServicesStart()
    {
        var builder = Host.CreateApplicationBuilder();
        var calls = new Calls();
        builder.Services.AddSingleton(calls);
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>(ServiceLifetime.Scoped);
        builder.Services.AddHostedService<Observer>();
        builder.Services.AddHttpClient("first").ConfigureBaseAddress("https://first.example.com/").AddPlatformAccessTokenHandler();
        builder.Services.AddHttpClient("second").ConfigureBaseAddress("https://second.example.com/").AddPlatformAccessTokenHandler();
        ServiceDefaultsHttpClientDependencyInjectionExtensions.AddPlatformAccessTokenValidationOnStart(builder.Services, "first");
        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        calls.RequestUris.ShouldBe([new Uri("https://first.example.com/"), new Uri("https://second.example.com/")], ignoreOrder: true);
        calls.Disposed.ShouldBe(2);
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SupportsDefaultClientWithoutBaseAddress()
    {
        var builder = Host.CreateApplicationBuilder();
        var calls = new Calls();
        builder.Services.AddSingleton(calls);
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>();
        builder.Services.AddHttpClient(string.Empty).AddPlatformAccessTokenHandler();
        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        calls.RequestUris.ShouldBe(new Uri?[] { null });
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TokenFailurePreventsStartup()
    {
        var builder = Host.CreateApplicationBuilder();
        var failure = new InvalidOperationException("Token generation failed");
        builder.Services.AddSingleton(new Calls { Failure = failure });
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>();
        builder.Services.AddHttpClient("api").AddPlatformAccessTokenHandler();
        using var host = builder.Build();

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
        thrown.ShouldBeSameAs(failure);
    }

    [Fact]
    public async Task PassesHostStartupCancellationToProvider()
    {
        var builder = Host.CreateApplicationBuilder();
        var calls = new Calls { WaitForCancellation = true };
        builder.Services.AddSingleton(calls);
        builder.Services.TryAddPlatformAccessTokenProvider<RecordingProvider>();
        builder.Services.AddHttpClient("api").AddPlatformAccessTokenHandler();
        using var host = builder.Build();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var startup = host.StartAsync(cancellation.Token);
        await calls.Called.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => startup);
    }

    private sealed class Calls
    {
        public List<Uri?> RequestUris { get; } = [];

        public Exception? Failure { get; init; }

        public bool WaitForCancellation { get; init; }

        public TaskCompletionSource Called { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Disposed { get; set; }
    }

    private sealed class RecordingProvider(Calls calls)
        : IPlatformAccessTokenProvider
        , IDisposable
        , IAsyncDisposable
    {
        public async ValueTask<string> GetPlatformAccessToken(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            calls.RequestUris.Add(request.RequestUri);
            calls.Called.TrySetResult();
            if (calls.WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            if (calls.Failure is { } failure)
            {
                throw failure;
            }

            return "token";
        }

        public void Dispose() => calls.Disposed++;

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class Observer(Calls calls)
        : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            calls.RequestUris.Count.ShouldBe(2);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NeverSendHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Startup validation must not send a request to the destination API.");
    }
}
