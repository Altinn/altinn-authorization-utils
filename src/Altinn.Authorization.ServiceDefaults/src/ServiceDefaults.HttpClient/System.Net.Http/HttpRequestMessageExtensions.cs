namespace System.Net.Http;

/// <summary>
/// Extension methods for <see cref="HttpRequestMessage"/> related to Altinn Authorization Service Defaults.
/// </summary>
public static class AltinnAuthorizationServiceDefaultsHttpRequestMessageExtensions
{
    private static readonly HttpRequestOptionsKey<bool> _disablePlatformAccessTokenOptionsKey = new("Altinn.PlatformAccessToken.Disable");

    /// <summary>
    /// Gets a value indicating whether the platform access token should be disabled for this request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to check for disabled status.</param>
    /// <returns><see langword="true"/> if platform access token should be disabled for this request, otherwise <see langword="false"/>.</returns>
    public static bool IsPlatformAccessTokenDisabled(this HttpRequestMessage request) 
        => request.Options.TryGetValue(_disablePlatformAccessTokenOptionsKey, out var disable) && disable;

    /// <summary>
    /// Disables setting a platform access token for this request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
    /// <param name="disable">If set to <see langword="true"/>, platform access token will be disabled for this request. Default is <see langword="true"/>.</param>
    public static void DisablePlatformAccessToken(this HttpRequestMessage request, bool disable = true)
    {
        request.Options.Set(_disablePlatformAccessTokenOptionsKey, true);
    }
}
