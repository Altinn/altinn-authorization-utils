using Altinn.Authorization.ServiceDefaults.Telemetry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Polly.CircuitBreaker;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Net;
using static System.Formats.Asn1.AsnWriter;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class HttpStandardResilienceTelemetryTests
    : IAsyncLifetime
{
    private WebApplication? _app;
    private AsyncServiceScope? _scope;
    private HttpClient? _client;
    private MetricCollector<int>? _openedCollector;
    private MetricCollector<int>? _closedCollector;
    private MetricCollector<int>? _halfOpenCollector;

    public async Task InitializeAsync()
    {
        var builder = AltinnHost.CreateWebApplicationBuilder("test", args: []);
        builder.Services.AddFakeLogging();
        builder.Services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => new FakeHandler());
        _app = builder.Build();

        await _app.StartAsync();

        var meterFactory = _app.Services.GetRequiredService<IMeterFactory>();
        _openedCollector = new(meterFactory, ServiceDefaultsMeter.MeterName, "http.standard_resilience.circuit_breaker.opened");
        _closedCollector = new(meterFactory, ServiceDefaultsMeter.MeterName, "http.standard_resilience.circuit_breaker.closed");
        _halfOpenCollector = new(meterFactory, ServiceDefaultsMeter.MeterName, "http.standard_resilience.circuit_breaker.half-opened");

        _scope = _app.Services.CreateAsyncScope();
        _client = _scope!.Value.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("test");
        _client.BaseAddress = new Uri("http://example.com/");
    }

    public async Task DisposeAsync()
    {
        if (_scope is { } scope)
        {
            await scope.DisposeAsync();
        }

        if (_app is { } app)
        {
            await app.DisposeAsync();
        }
    }

    [Fact]
    public async Task CircuitBreakerOpened_DoesNotThrow()
    {
        var baseUri = new Uri("http://example.com/");
        await Should.ThrowAsync<BrokenCircuitException>(() => Parallel.ForEachAsync(Enumerable.Range(0, 100), async (_, ct) =>
        {
            await using var scope = _app!.Services.CreateAsyncScope();
            var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("test");
            client.BaseAddress = baseUri;
            var response = await _client!.GetAsync("/503");
        }));
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = request.RequestUri?.LocalPath switch
            {
                "/200" => new HttpResponseMessage(HttpStatusCode.OK),
                "/500" => new HttpResponseMessage(HttpStatusCode.InternalServerError),
                "/503" => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                _ => throw new NotImplementedException()
            };
            
            return Task.FromResult(response);
        }
    }
}
