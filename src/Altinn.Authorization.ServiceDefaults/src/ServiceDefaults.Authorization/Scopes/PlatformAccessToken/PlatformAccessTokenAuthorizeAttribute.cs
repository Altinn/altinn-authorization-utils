namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Specifies an authorization requirement that enforces platform access token validation for incoming requests.
/// </summary>
public sealed class PlatformAccessTokenAuthorizeAttribute
    : AuthorizationRequirementAttribute
    , IPlatformAccessTokenRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenAuthorizeAttribute"/> class with default settings that allow
    /// all issuers.
    /// </summary>
    public PlatformAccessTokenAuthorizeAttribute()
        : this(ApprovedIssuersCheck.AllowAll)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenAuthorizeAttribute"/> class using the specified set of
    /// approved platform access token issuers.
    /// </summary>
    /// <param name="approvedIssuers">A collection of well-known platform access token issuers that are considered approved for authorization. Only
    /// tokens issued by these issuers will be accepted. Requires at least one <see cref="WellKnownPlatformAccessTokenIssuers"/> to be set.</param>
    public PlatformAccessTokenAuthorizeAttribute(WellKnownPlatformAccessTokenIssuers approvedIssuers)
        : this(ApprovedIssuersCheck.Create(approvedIssuers))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenAuthorizeAttribute"/> class with the specified list of
    /// approved token issuers.
    /// </summary>
    /// <param name="approvedIssuers">An array of strings containing the names of token issuers that are approved for authorization. If empty,
    /// all issuers are approved.</param>
    public PlatformAccessTokenAuthorizeAttribute(params string[] approvedIssuers)
        : this(ApprovedIssuersCheck.Create(approvedIssuers))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlatformAccessTokenAuthorizeAttribute"/> class with the specified approved
    /// issuers check.
    /// </summary>
    /// <param name="approvedIssuers">The <see cref="ApprovedIssuersCheck"/> instance used to validate whether a token's issuer is approved. Cannot be null.</param>
    public PlatformAccessTokenAuthorizeAttribute(ApprovedIssuersCheck approvedIssuers)
    {
        ApprovedIssuers = approvedIssuers;
    }

    /// <inheritdoc/>
    public ApprovedIssuersCheck ApprovedIssuers { get; }

    /// <inheritdoc/>
    public override string ToString()
        => $"{nameof(PlatformAccessTokenAuthorizeAttribute)}: Requires platform-access-token";
}
