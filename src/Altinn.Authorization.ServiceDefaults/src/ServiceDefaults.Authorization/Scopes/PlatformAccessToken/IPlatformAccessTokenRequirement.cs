using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Defines a requirement for platform access tokens, including validation of approved token issuers.
/// </summary>
/// <remarks>Implementations of this interface are used to specify authorization policies that require access
/// tokens issued by approved platforms. This interface is typically used in conjunction with authorization handlers to
/// enforce issuer validation in authentication flows.</remarks>
public interface IPlatformAccessTokenRequirement
    : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the configuration that determines which certificate issuers are considered approved during validation.
    /// </summary>
    public ApprovedIssuersCheck ApprovedIssuers { get; }
}
