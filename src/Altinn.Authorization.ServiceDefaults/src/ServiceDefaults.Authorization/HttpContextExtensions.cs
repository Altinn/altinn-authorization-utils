using Microsoft.AspNetCore.Http;

namespace Altinn.Authorization.ServiceDefaults.Authorization;

/// <summary>
/// Extensions for <see cref="HttpContext"/>.
/// </summary>
internal static class HttpContextExtensions
{
    private const string Prefix = $"{nameof(ServiceDefaults)}.{nameof(Authorization)}.{nameof(HttpContextExtensions)}";
    private static readonly string PlatformTokenInformationItemKey = $"{Prefix}:{nameof(PlatformTokenInformationItemKey)}";

    /// <param name="context">The <see cref="HttpContext"/>.</param>
    extension(HttpContext context)
    {
        public (string Issuer, string? App)? PlatformTokenMetadata
        {
            get => context.Items.TryGetValue(PlatformTokenInformationItemKey, out var value)
                && value is PlatformTokenMetadata metadata
                ? (metadata.Issuer, metadata.App)
                : null;

            set => context.Items[PlatformTokenInformationItemKey] =
                value is { App: var app, Issuer: var issuer }
                ? new PlatformTokenMetadata(issuer, app)
                : null;
        }
    }

    private sealed record PlatformTokenMetadata(string Issuer, string? App);
}
