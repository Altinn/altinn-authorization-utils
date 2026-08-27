using System.Security.Claims;

namespace Altinn.Authorization.TestUtils.AspNetCore.Authentication;

internal sealed record JsonClaimsIdentity(IEnumerable<JsonClaim> Claims, string? AuthenticationType)
{
    public static JsonClaimsIdentity FromIdentity(ClaimsIdentity identity) => new(identity.Claims.Select(JsonClaim.FromClaim), identity.AuthenticationType);
    public ClaimsIdentity ToIdentity() => new(Claims.Select(c => c.ToClaim()), AuthenticationType);
}
