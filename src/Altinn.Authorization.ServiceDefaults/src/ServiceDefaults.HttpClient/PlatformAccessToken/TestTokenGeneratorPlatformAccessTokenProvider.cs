using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Provides a platform access token provider that retrieves tokens from the Altinn TestTokenGenerator service for use
/// in test environments.
/// </summary>
/// <remarks>This implementation is intended for use in testing scenarios where platform access tokens are
/// required but should not be obtained from production sources. It uses the configured test token generator service and
/// settings to issue tokens suitable for test purposes.</remarks>
internal sealed partial class TestTokenGeneratorPlatformAccessTokenProvider
    : IPlatformAccessTokenProvider
{
    private readonly System.Net.Http.HttpClient _client;
    private readonly IOptions<AltinnTestTokenGeneratorSettings> _options;
    private readonly ILogger<TestTokenGeneratorPlatformAccessTokenProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestTokenGeneratorPlatformAccessTokenProvider"/> class.
    /// </summary>
    public TestTokenGeneratorPlatformAccessTokenProvider(
        System.Net.Http.HttpClient client,
        IOptions<AltinnTestTokenGeneratorSettings> options,
        ILogger<TestTokenGeneratorPlatformAccessTokenProvider> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<string> GetPlatformAccessToken(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        var url = Url.Create(
            "api/GetPlatformAccessToken",
            [
                new("env", options.EnvName),
                new("app", options.AppName),
                new("ttl", "300"), // 5 minute token duration
            ]);

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Get, url);
        tokenRequest.Headers.Authorization = options.Authentication;

        Log.GettingTestToken(_logger, options.AppName, options.EnvName, request.RequestUri?.Host);
        using var tokenResponse = await _client.SendAsync(tokenRequest, cancellationToken);

        Log.TokenResponseStatusCode(_logger, tokenResponse.StatusCode);
        tokenResponse.EnsureSuccessStatusCode();

        var token = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        return token;
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Debug, "Getting test token for {AppName} in {EnvName} for request to {RequestHost}")]
        public static partial void GettingTestToken(ILogger logger, string? appName, string? envName, string? requestHost);

        [LoggerMessage(1, LogLevel.Debug, "Token response status code: {StatusCode}")]
        public static partial void TokenResponseStatusCode(ILogger logger, HttpStatusCode statusCode);
    }
}
