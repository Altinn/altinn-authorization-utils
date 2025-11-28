namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Represents a platform-access-token requirement that allows all issuers.
/// </summary>
internal sealed class PlatformAccessTokenRequirement
    : IPlatformAccessTokenRequirement
{
    public static PlatformAccessTokenRequirement AllowAll { get; } = new(ApprovedIssuersCheck.AllowAll);

    public PlatformAccessTokenRequirement(ApprovedIssuersCheck approvedIssuers)
    {
        ApprovedIssuers = approvedIssuers;
    }

    /// <inheritdoc/>
    public ApprovedIssuersCheck ApprovedIssuers { get; }
}
