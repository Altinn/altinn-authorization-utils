using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if NET9_0_OR_GREATER
using Microsoft.Extensions.Caching.Hybrid;
#else
using Microsoft.Extensions.Caching.Memory;
#endif

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

[ExcludeFromCodeCoverage]
internal sealed class PlatformAccessTokenSigningKeyProvider
    : BasePlatformAccessTokenSigningKeyProvider
{
    private readonly SecretClient _secretClient;

    public PlatformAccessTokenSigningKeyProvider(
        IOptionsMonitor<PlatformAccessTokenSettings> settings,
        [FromKeyedServices(typeof(PlatformAccessTokenSettings))] SecretClient secretClient,
#if NET9_0_OR_GREATER
        HybridCache cache
#else
        IMemoryCache cache
#endif
        )
        : base(settings, cache)
    {
        _secretClient = secretClient;
    }

    /// <inheritdoc/>
    protected override async IAsyncEnumerable<X509Certificate2> GetSigningCertificates(string issuer, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var secretName = $"{issuer}-access-token-public-cert";
        var keyVaultSecret = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);

        X509Certificate2Collection certs = [];
#if NET9_0_OR_GREATER
        certs.Add(X509CertificateLoader.LoadCertificate(Convert.FromBase64String(keyVaultSecret.Value.Value)));
#else
        certs.Import(Convert.FromBase64String(keyVaultSecret.Value.Value));
#endif

        foreach (var cert in certs)
        {
            yield return cert;
        }
    }
}
