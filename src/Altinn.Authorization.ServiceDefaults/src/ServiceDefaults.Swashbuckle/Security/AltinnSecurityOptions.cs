namespace Altinn.Authorization.ServiceDefaults.Swashbuckle.Security;

/// <summary>
/// Provides configuration options for Altinn authentication schemes in Swagger documentation.
/// </summary>
public sealed class AltinnSecurityOptions
{
    /// <summary>
    /// Gets the default authentication scheme name for Altinn OIDC integration.
    /// </summary>
    public static string DefaultAltinnOidcSchemeName => "altinn";

    /// <summary>
    /// Gets the default authentication scheme name for platform tokens.
    /// </summary>
    public static string DefaultPlatformTokenSchemeName => "altinn-platform-token";

    /// <summary>
    /// Gets the default scheme name used for Azure API Management authentication.
    /// </summary>
    public static string DefaultAPIMSchemeName => "apim";

    /// <summary>
    /// Gets a value indicating whether the Altinn OIDC authentication scheme is enabled by default.
    /// </summary>
    public static bool DefaultEnableAltinnOidcScheme => true;

    /// <summary>
    /// Gets a value indicating whether the platform token authentication scheme is enabled by default.
    /// </summary>
    public static bool DefaultEnablePlatformTokenScheme => true;

    /// <summary>
    /// Gets a value indicating whether the Azure API Management scheme is enabled by default.
    /// </summary>
    public static bool DefaultEnableAPIMSCheme => true;

    /// <summary>
    /// Gets the default relative URI for the Altinn OpenID Connect configuration-endpoint.
    /// </summary>
    public static Uri DefaultAltinnOidcConfigurationUri { get; } = new Uri("/authentication/api/v1/openid/.well-known/openid-configuration", UriKind.Relative);

    /// <summary>
    /// Gets or sets a value indicating whether the Altinn OIDC authentication scheme is enabled.
    /// </summary>
    public bool? EnableAltinnOidcScheme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the authentication scheme name for Altinn OIDC integration.
    /// </summary>
    public string? AltinnOidcSchemeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the platform token authentication scheme is enabled.
    /// </summary>
    public bool? EnablePlatformTokenScheme { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the authentication scheme name for platform tokens.
    /// </summary>
    public string? PlatformTokenSchemeName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Azure API Management scheme is enabled.
    /// </summary>
    public bool? EnableAPIMScheme { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure API Management scheme to use for requests.
    /// </summary>
    public string? APIMSchemeName { get; set; }

    /// <summary>
    /// Gets or sets the URI of the Altinn OIDC configuration-endpoint.
    /// </summary>
    public Uri? AltinnOidcConfigurationUri { get; set; }
}
