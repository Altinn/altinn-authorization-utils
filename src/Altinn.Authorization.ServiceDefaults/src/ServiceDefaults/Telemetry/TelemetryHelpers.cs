using Altinn.Authorization.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

[ExcludeFromCodeCoverage]
internal static class TelemetryHelpers
{
    private readonly static SearchValues<string> AuthenticationLevel = SearchValues.Create(["urn:altinn:authlevel"], StringComparison.Ordinal);
    private readonly static SearchValues<string> UserId = SearchValues.Create(["urn:altinn:userid"], StringComparison.Ordinal);
    private readonly static SearchValues<string> PartyID = SearchValues.Create(["urn:altinn:partyid"], StringComparison.Ordinal);
    private readonly static SearchValues<string> OrgNumber = SearchValues.Create(["urn:altinn:orgNumber"], StringComparison.Ordinal);
    private readonly static SearchValues<string> ClientId = SearchValues.Create(["client_id"], StringComparison.Ordinal);

    public static bool ShouldExclude(Uri url)
        => ShouldExclude(url.LocalPath.AsSpan());

    public static bool ShouldExclude(PathString localPath)
        => ShouldExclude(localPath.HasValue ? localPath.Value.AsSpan() : []);

    private static bool ShouldExclude(ReadOnlySpan<char> localPath)
    {
        while (localPath.Length > 0 && localPath[^1] == '/')
        {
            localPath = localPath[..^1];
        }

        if (localPath.EndsWith(AltinnServiceDefaultsExtensions.HealthEndpoint, StringComparison.Ordinal))
        {
            return true;
        }

        if (localPath.EndsWith(AltinnServiceDefaultsExtensions.AliveEndpoint, StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    public static void EnrichFromRequest(Activity activity, HttpContext ctx)
    {
        if (ctx?.User is { } user)
        {
            bool hasAuthLevel = false, hasPartyId = false, hasUserId = false, hasOrgNumber = false, hasClientId = false;
            foreach (var claim in user.Claims)
            {
                if (AuthenticationLevel.Contains(claim.Type))
                {
                    if (!hasAuthLevel && int.TryParse(claim.Value, out var authLevel))
                    {
                        hasAuthLevel = true;
                        activity.SetTag("altinn.auth_level", authLevel);
                    }

                    continue;
                }
                
                if (PartyID.Contains(claim.Type))
                {
                    if (!hasPartyId && int.TryParse(claim.Value, out var partyId))
                    {
                        hasPartyId = true;
                        activity.SetTag("altinn.party.id", partyId);
                    }

                    continue;
                }
                
                if (UserId.Contains(claim.Type))
                {
                    if (!hasUserId && int.TryParse(claim.Value, out var userId))
                    {
                        hasUserId = true;
                        activity.SetTag("altinn.user.id", userId);
                    }

                    continue;
                }
                
                if (OrgNumber.Contains(claim.Type))
                {
                    if (!hasOrgNumber && int.TryParse(claim.Value, out var orgNumber))
                    {
                        hasOrgNumber = true;
                        activity.SetTag("altinn.org.number", claim.Value);
                    }

                    continue;
                }

                if (ClientId.Contains(claim.Type))
                {
                    if (!hasClientId)
                    {
                        hasClientId = true;
                        activity.SetTag("altinn.client.id", claim.Value);
                    }

                    continue;
                }
            }
        }

        if (ctx?.PlatformTokenMetadata is { App: var app, Issuer: var issuer })
        {
            activity.SetTag("altinn.platform_token.issuer", issuer);

            if (app is not null)
            {
                activity.SetTag("altinn.platform_token.app", app);
            }
        }
    }

    public static void EnrichFromRequest(IHttpMetricsTagsFeature metricsTagsFeature, HttpContext ctx)
    {
        if (ctx?.User is { } user)
        {
            foreach (var claim in user.Claims)
            {
                if (ClientId.Contains(claim.Type))
                {
                    metricsTagsFeature.Tags.Add(new("altinn.client.id", claim.Value));
                    break;
                }
            }
        }
    }
}
