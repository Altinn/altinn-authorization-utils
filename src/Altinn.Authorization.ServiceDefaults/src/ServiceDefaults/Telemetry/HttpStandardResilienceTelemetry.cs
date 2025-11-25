using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

internal sealed partial class HttpStandardResilienceTelemetry
{
    private readonly ILogger<HttpStandardResilienceTelemetry> _logger;
    private readonly Meters _meters;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpStandardResilienceTelemetry"/> class.
    /// </summary>
    public HttpStandardResilienceTelemetry(
        ILogger<HttpStandardResilienceTelemetry> logger,
        IMetricsProvider metrics)
    {
        _logger = logger;
        _meters = metrics.Get<Meters>();
    }

    public void CircuitBreakerOpened(string httpClientName, in OnCircuitOpenedArguments<HttpResponseMessage> context)
    {
        var statusCode = context.Outcome.Result?.StatusCode;

        _meters.CircuitBreakerOpened(httpClientName, context.Context.OperationKey);
        Log.CircuitBreakerOpened(_logger, httpClientName, statusCode.HasValue ? (int)statusCode.Value : -1, context.BreakDuration, context.Outcome.Exception);
    }

    public void CircuitBreakerHalfOpen(string httpClientName, in OnCircuitHalfOpenedArguments context)
    {
        _meters.CircuitBreakerHalfOpen(httpClientName, context.Context.OperationKey);
        Log.CircuitBreakerHalfOpen(_logger, httpClientName);
    }

    public void CircuitBreakerClosed(string httpClientName, in OnCircuitClosedArguments<HttpResponseMessage> context)
    {
        _meters.CircuitBreakerClosed(httpClientName, context.Context.OperationKey);
        Log.CircuitBreakerClosed(_logger, httpClientName);
    }

    private sealed class Meters
        : IMetrics<Meters>
    {
        public static Meters Create(Meter meter)
            => new(meter);

        private readonly Counter<int> _circuitBreakerOpened;
        private readonly Counter<int> _circuitBreakerClosed;
        private readonly Counter<int> _circuitBreakerHalfOpen;

        private Meters(Meter meter)
        {
            _circuitBreakerOpened = meter.CreateCounter<int>(
                "http.standard_resilience.circuit_breaker.opened",
                unit: null,
                description: "Number of times the circuit breaker has opened");

            _circuitBreakerHalfOpen = meter.CreateCounter<int>(
                "http.standard_resilience.circuit_breaker.half-opened",
                unit: null,
                description: "Number of times the circuit breaker has half-opened");

            _circuitBreakerClosed = meter.CreateCounter<int>(
                "http.standard_resilience.circuit_breaker.closed",
                unit: null,
                description: "Number of times the circuit breaker has closed");
        }

        public void CircuitBreakerOpened(string httpClientName, string? pipelineContextKey)
            => _circuitBreakerOpened.Add(1, [new("http.client.name", httpClientName), new("pipeline.context.key", pipelineContextKey)]);

        public void CircuitBreakerHalfOpen(string httpClientName, string? pipelineContextKey)
            => _circuitBreakerHalfOpen.Add(1, [new("http.client.name", httpClientName), new("pipeline.context.key", pipelineContextKey)]);

        public void CircuitBreakerClosed(string httpClientName, string? pipelineContextKey)
            => _circuitBreakerClosed.Add(1, [new("http.client.name", httpClientName), new("pipeline.context.key", pipelineContextKey)]);
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Error, "Circuit breaker opened for {HttpClientName}. Response status: {ResponseStatusCode}. Circuit will be closed for: {BreakDuration}.")]
        public static partial void CircuitBreakerOpened(ILogger logger, string httpClientName, int responseStatusCode, TimeSpan breakDuration, Exception? exception);

        [LoggerMessage(1, LogLevel.Information, "Circuit breaker half-open for {HttpClientName}.")]
        public static partial void CircuitBreakerHalfOpen(ILogger logger, string httpClientName);

        [LoggerMessage(2, LogLevel.Information, "Circuit breaker closed for {HttpClientName}.")]
        public static partial void CircuitBreakerClosed(ILogger logger, string httpClientName);
    }
}
