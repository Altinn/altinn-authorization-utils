using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Caching.Hybrid;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Provides a base implementation for retrieving and caching signing keys used to validate platform access tokens.
/// </summary>
public abstract class BasePlatformAccessTokenSigningKeyProvider
    : IPlatformAccessTokenSigningKeyProvider
{
    private readonly IOptionsMonitor<PlatformAccessTokenSettings> _settings;

    private readonly HybridCache _cache;

    /// <summary>
    /// Initializes a new instance of the BasePlatformAccessTokenSigningKeyProvider class with the specified platform
    /// access token settings and cache implementation.
    /// </summary>
    protected BasePlatformAccessTokenSigningKeyProvider(
        IOptionsMonitor<PlatformAccessTokenSettings> settings,
        HybridCache cache
        )
    {
        _settings = settings;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<SecurityKey> GetSigningKeys(
        string issuer,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var cacheKey = $"/{nameof(DefaultPlatformAccessTokenSigningKeyProvider)}/pub-keys/{issuer}";
        var settings = _settings.CurrentValue;
        string? keyData;

        keyData = await _cache.GetOrCreateAsync(
            key: cacheKey,
            state: (Self: this, Issuer: issuer),
            factory: (state, cancellationToken) => GetKeyData(state.Self, state.Issuer, cancellationToken),
            options: new HybridCacheEntryOptions { Expiration = TimeSpan.FromSeconds(settings.CacheCertLifetimeInSeconds) },
            cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(keyData))
        {
            yield break;
        }

        X509Certificate2Collection certs = [];
        certs.ImportFromPem(keyData.AsSpan());

        foreach (var cert in certs)
        {
            yield return new X509SecurityKey(cert);
        }

        static async ValueTask<string> GetKeyData(
            BasePlatformAccessTokenSigningKeyProvider provider,
            string issuer,
            CancellationToken cancellationToken)
        {
            X509Certificate2Collection certs = [];
            await foreach (var cert in provider.GetSigningCertificates(issuer, cancellationToken))
            {
                certs.Add(cert);
            }

            if (certs.Count == 0)
            {
                return string.Empty;
            }

            return certs.ExportCertificatePems();
        }
    }

    /// <summary>
    /// Asynchronously retrieves the collection of signing certificates associated with the specified issuer.
    /// </summary>
    /// <param name="issuer">The unique identifier of the issuer whose signing certificates are to be retrieved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous stream of <see cref="X509Certificate2"/> objects representing the signing certificates for the
    /// specified issuer. The stream will be empty if no certificates are found.</returns>
    protected abstract IAsyncEnumerable<X509Certificate2> GetSigningCertificates(string issuer, CancellationToken cancellationToken);
}
