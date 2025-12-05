using Microsoft.IdentityModel.Tokens;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Provides a mechanism for retrieving signing keys used for validating platform access tokens.
/// </summary>
public interface IPlatformAccessTokenSigningKeyProvider
{
    /// <summary>
    /// Gets the signing keys for the specified issuer.
    /// </summary>
    /// <remarks>
    /// Should not throw if issuer is unknown, just return an empty enumeration.
    /// </remarks>
    /// <param name="issuer">The issuer to get security keys for.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>An async enumerable of <see cref="SecurityKey"/>s.</returns>
    public IAsyncEnumerable<SecurityKey> GetSigningKeys(string issuer, CancellationToken cancellationToken);
}
