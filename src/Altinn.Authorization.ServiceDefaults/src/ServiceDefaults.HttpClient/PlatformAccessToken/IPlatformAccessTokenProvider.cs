namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

/// <summary>
/// Defines a contract for obtaining a platform-access-token for an HTTP request.
/// </summary>
public interface IPlatformAccessTokenProvider
{
    /// <summary>
    /// Asynchronously retrieves a platform access token associated with the specified HTTP request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage" /> for which to obtain the platform access token. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A platform-access-token.</returns>
    ValueTask<string> GetPlatformAccessToken(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
