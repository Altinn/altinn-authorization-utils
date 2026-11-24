using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten.Tests;

public class MaskinPortenStartupValidatorTests
{
    [Theory]
    [InlineData("test-client", null, true)]
    [InlineData("test-client", true, true)]
    [InlineData("test-client", false, false)]
    [InlineData(null, null, false)]
    [InlineData(null, true, false)]
    public async Task HandlerValidationRespectsClientNameAndOptOut(string? clientName, bool? validateOnStart, bool expectedValidation)
    {
        var builder = Host.CreateApplicationBuilder();
        var client = new RecordingClient();
        builder.Services.AddSingleton<IMaskinPortenClient>(client);
        var httpClient = builder.Services.AddHttpClient("api");
        if (validateOnStart is { } validate)
        {
            httpClient.AddMaskinPortenHandler(clientName, validateOnStart: validate);
        }
        else
        {
            httpClient.AddMaskinPortenHandler(clientName);
        }

        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
        if (expectedValidation)
        {
            client.Names.ShouldBe([clientName!]);
        }
        else
        {
            client.Names.ShouldBeEmpty();
        }

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidationIsOptIn()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddMaskinPortenClient();
        var client = new RecordingClient();
        builder.Services.AddSingleton<IMaskinPortenClient>(client);
        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        client.Names.ShouldBeEmpty();
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ValidatesEachNamedClientOnceBeforeServicesStart()
    {
        var builder = Host.CreateApplicationBuilder();
        var client = new RecordingClient();
        builder.Services.AddSingleton<IMaskinPortenClient>(client);
        builder.Services.AddHostedService<Observer>(services => new Observer(client));
        HttpClientMaskinPortenDependencyInjectionExtensions.AddMaskinPortenClientValidationOnStart(builder.Services, "first");
        HttpClientMaskinPortenDependencyInjectionExtensions.AddMaskinPortenClientValidationOnStart(builder.Services, "second");
        HttpClientMaskinPortenDependencyInjectionExtensions.AddMaskinPortenClientValidationOnStart(builder.Services, "first");
        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        client.Names.ShouldBe(["first", "second"], ignoreOrder: true);
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TokenFailurePreventsStartup()
    {
        var builder = Host.CreateApplicationBuilder();
        var failure = new InvalidOperationException("Token request failed");
        var client = new RecordingClient { Failure = failure };
        builder.Services.AddSingleton<IMaskinPortenClient>(client);
        HttpClientMaskinPortenDependencyInjectionExtensions.AddMaskinPortenClientValidationOnStart(builder.Services, "broken");
        using var host = builder.Build();

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => host.StartAsync(TestContext.Current.CancellationToken));
        thrown.ShouldBeSameAs(failure);
        client.Names.ShouldBe(["broken"]);
    }

    [Fact]
    public async Task PassesStartupCancellationToClient()
    {
        var builder = Host.CreateApplicationBuilder();
        var client = new RecordingClient { WaitForCancellation = true };
        builder.Services.AddSingleton<IMaskinPortenClient>(client);
        HttpClientMaskinPortenDependencyInjectionExtensions.AddMaskinPortenClientValidationOnStart(builder.Services, "cancelled");
        using var host = builder.Build();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        var startup = host.StartAsync(cancellation.Token);
        await client.Called.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        await Should.ThrowAsync<OperationCanceledException>(() => startup);
    }

    private sealed class Observer(RecordingClient client)
        : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            client.Names.Count.ShouldBe(2);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class RecordingClient
        : IMaskinPortenClient
    {
        public List<string> Names { get; } = [];

        public Exception? Failure { get; init; }

        public bool WaitForCancellation { get; init; }

        public TaskCompletionSource Called { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<MaskinPortenToken> GetAccessToken(string clientName, CancellationToken cancellationToken = default)
        {
            Names.Add(clientName);
            Called.TrySetResult();
            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            if (Failure is { } failure)
            {
                throw failure;
            }

            return new MaskinPortenToken(new MaskinPortenCacheKey(clientName, "id", "scope", null, null), "token", DateTimeOffset.UtcNow.AddMinutes(2));
        }
    }
}
