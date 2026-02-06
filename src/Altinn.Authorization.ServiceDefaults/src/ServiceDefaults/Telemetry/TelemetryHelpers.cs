using Altinn.Authorization.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Buffers;
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

    public static void EnrichFromRequest<T>(T tags, HttpContext ctx)
        where T : ITags
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
                        tags["altinn.auth_level"] = authLevel.ToString();
                    }

                    continue;
                }
                
                if (PartyID.Contains(claim.Type))
                {
                    if (!hasPartyId && int.TryParse(claim.Value, out var partyId))
                    {
                        hasPartyId = true;
                        tags["altinn.party_id"] = partyId.ToString();
                    }

                    continue;
                }
                
                if (UserId.Contains(claim.Type))
                {
                    if (!hasUserId && int.TryParse(claim.Value, out var userId))
                    {
                        hasUserId = true;
                        tags["altinn.user_id"] = userId.ToString();
                    }

                    continue;
                }
                
                if (OrgNumber.Contains(claim.Type))
                {
                    if (!hasOrgNumber && int.TryParse(claim.Value, out var orgNumber))
                    {
                        hasOrgNumber = true;
                        tags["altinn.org_number"] = orgNumber.ToString();
                    }

                    continue;
                }

                if (ClientId.Contains(claim.Type))
                {
                    if (!hasClientId)
                    {
                        hasClientId = true;
                        tags["altinn.client_id"] = claim.Value;
                    }

                    continue;
                }
            }
        }

        if (ctx?.PlatformTokenMetadata is { App: var app, Issuer: var issuer })
        {
            tags["altinn.platform_token.issuer"] = issuer;

            if (app is not null)
            {
                tags["altinn.platform_token.app"] = app;
            }
        }
    }

    public interface ITags
    {
        string this[string key] { set; }
    }
}
