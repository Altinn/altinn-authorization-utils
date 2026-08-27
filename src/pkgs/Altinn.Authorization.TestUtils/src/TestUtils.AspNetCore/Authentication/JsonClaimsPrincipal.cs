using System.Security.Claims;

namespace Altinn.Authorization.TestUtils.AspNetCore.Authentication;

internal sealed record JsonClaimsPrincipal(IEnumerable<JsonClaimsIdentity> Identities)
{
    public static JsonClaimsPrincipal FromPrincipal(ClaimsPrincipal principal) => new(principal.Identities.Select(JsonClaimsIdentity.FromIdentity));
    public ClaimsPrincipal ToPrincipal() => new(Identities.Select(i => i.ToIdentity()));
}
