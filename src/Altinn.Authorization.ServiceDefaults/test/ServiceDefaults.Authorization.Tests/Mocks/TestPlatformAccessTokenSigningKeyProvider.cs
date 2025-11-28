using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Immutable;
using System.Security.Cryptography;

#if NET9_0_OR_GREATER
using Microsoft.Extensions.Caching.Hybrid;
#else
using Microsoft.Extensions.Caching.Memory;
#endif

namespace Altinn.Authorization.ServiceDefaults.Authorization.Tests.Mocks;

internal sealed class TestPlatformAccessTokenSigningKeyProvider
    : BasePlatformAccessTokenSigningKeyProvider
{
    private ImmutableDictionary<string, ImmutableList<X509Certificate2>> _certs
        = ImmutableDictionary<string, ImmutableList<X509Certificate2>>.Empty;

    public TestPlatformAccessTokenSigningKeyProvider(
        PlatformAccessTokenSettings settings,
#if NET9_0_OR_GREATER
        HybridCache cache
#else
        IMemoryCache cache
#endif
        )
        : base(new TestOptionsMonitor<PlatformAccessTokenSettings>(settings), cache)
    {
    }

    public TestPlatformAccessTokenSigningKeyProvider()
        : this(
              new PlatformAccessTokenSettings(),
#if NET9_0_OR_GREATER
              new TestHybridCache()
#else
              new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions()))
#endif              
              )
    {
    }

    protected override async IAsyncEnumerable<X509Certificate2> GetSigningCertificates(string issuer, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_certs.TryGetValue(issuer, out var bag))
        {
            yield break;
        }

        foreach (var cert in bag)
        {
            yield return cert;
        }
    }

    public X509Certificate2 GetOrCreateCertificate(string issuer)
        => ImmutableInterlocked.GetOrAdd(
            ref _certs,
            issuer,
            static (issuer) =>
            {
                var cert = CreateCertificate(issuer);
                return [cert];
            })[0];

    public X509Certificate2 CreateOrRotateCertificate(string issuer)
        => ImmutableInterlocked.AddOrUpdate(
            ref _certs,
            issuer,
            static (issuer) =>
            {
                var cert = CreateCertificate(issuer);
                return [cert];
            },
            static (issuer, existing) =>
            {
                var cert = CreateCertificate(issuer);
                while (existing.Count > 2)
                {
                    existing = existing.RemoveAt(0);
                }

                return existing.Add(cert);
            })[^1];

    public void Delete(string issuer)
    {
        ImmutableInterlocked.TryRemove(ref _certs, issuer, out _);
    }

    private static X509Certificate2 CreateCertificate(string issuer)
    {
        X500DistinguishedName dn = new($"CN={issuer}");
        using RSA rsa = RSA.Create(2048);

        CertificateRequest request = new(dn, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1"), new Oid("1.3.6.1.5.5.7.3.2") }, true));

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
        return certificate;
    }
}
