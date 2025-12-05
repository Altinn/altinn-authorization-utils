using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;

/// <summary>
/// Settings for Key Vault used to retrieve signing keys
/// </summary>
public sealed class PlatformAccessTokenKeyVaultSettings
{
    /// <summary>
    /// The key vault reader client id
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// The key vault client secret
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// The key vault tenant Id
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// The uri to the key vault
    /// </summary>
    [Required]
    public Uri? SecretUri { get; set; }
}
