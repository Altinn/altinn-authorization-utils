using DnsClient.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

[ExcludeFromCodeCoverage]
internal static class TelemetryHelpers
{
    private readonly static string AuthenticationLevel = "urn:altinn:authlevel";
    private readonly static string UserId = "urn:altinn:userid";
    private readonly static string PartyID = "urn:altinn:partyid";
    private readonly static string OrgNumber = "urn:altinn:orgNumber";

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
            bool hasAuthLevel = false, hasPartyId = false, hasUserId = false, hasOrgNumber = false;
            foreach (var claim in user.Claims)
            {
                if (string.Equals(claim.Type, AuthenticationLevel, StringComparison.Ordinal))
                {
                    if (!hasAuthLevel && int.TryParse(claim.Value, out var authLevel))
                    {
                        hasAuthLevel = true;
                        tags["altinn.auth_level"] = authLevel.ToString();
                    }

                    continue;
                }
                
                if (string.Equals(claim.Type, PartyID, StringComparison.Ordinal))
                {
                    if (!hasPartyId && int.TryParse(claim.Value, out var partyId))
                    {
                        hasPartyId = true;
                        tags["altinn.party_id"] = partyId.ToString();
                    }

                    continue;
                }
                
                if (string.Equals(claim.Type, UserId, StringComparison.Ordinal))
                {
                    if (!hasUserId && int.TryParse(claim.Value, out var userId))
                    {
                        hasUserId = true;
                        tags["altinn.user_id"] = userId.ToString();
                    }

                    continue;
                }
                
                if (string.Equals(claim.Type, OrgNumber, StringComparison.Ordinal))
                {
                    if (!hasOrgNumber && int.TryParse(claim.Value, out var orgNumber))
                    {
                        hasOrgNumber = true;
                        tags["altinn.org_number"] = orgNumber.ToString();
                    }

                    continue;
                }
            }
        }
    }

    public interface ITags
    {
        string this[string key] { set; }
    }
}
