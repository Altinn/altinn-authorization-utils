using CommunityToolkit.Diagnostics;
using System.Net.Http;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// A <see cref="DelegatingHandler"/> that adds a platform token to outbound requests.
/// </summary>
internal class PlatformAccessTokenHandler
    : AsyncOnlyDelegatingHandler
{
    private static readonly string PlatformAccessTokenHeaderName = "PlatformAccessToken";

    private readonly IPlatformAccessTokenProvider _tokenProvider;

    /// <summary>
    /// Initialize a new <see cref="PlatformAccessTokenHandler"/>.
    /// </summary>
    public PlatformAccessTokenHandler(IPlatformAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Guard.IsNotNull(request);

        if (request.Headers.Contains(PlatformAccessTokenHeaderName))
        {
            return base.SendAsync(request, cancellationToken);
        }

        if (request.IsPlatformAccessTokenDisabled())
        {
            return base.SendAsync(request, cancellationToken);
        }

        return AddPlatformTokenAndSendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> AddPlatformTokenAndSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token;

        {
            using var activity = HttpClientTelemetry.ActivitySource.StartActivity("get platform-access-token");
            token = await _tokenProvider.GetPlatformAccessToken(request, cancellationToken);
        }

        request.Headers.Add(PlatformAccessTokenHeaderName, token);
        return await base.SendAsync(request, cancellationToken);
    }
}
