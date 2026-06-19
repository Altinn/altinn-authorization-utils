using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Caching.Hybrid;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

[ExcludeFromCodeCoverage]
internal sealed class DefaultPlatformAccessTokenSigningKeyProvider
    : BasePlatformAccessTokenSigningKeyProvider
{
    private readonly SecretClient _secretClient;

    public DefaultPlatformAccessTokenSigningKeyProvider(
        IOptionsMonitor<PlatformAccessTokenSettings> settings,
        [FromKeyedServices(typeof(PlatformAccessTokenSettings))] SecretClient secretClient,
        HybridCache cache
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
        certs.Add(X509CertificateLoader.LoadCertificate(Convert.FromBase64String(keyVaultSecret.Value.Value)));

        foreach (var cert in certs)
        {
            yield return cert;
        }
    }
}
