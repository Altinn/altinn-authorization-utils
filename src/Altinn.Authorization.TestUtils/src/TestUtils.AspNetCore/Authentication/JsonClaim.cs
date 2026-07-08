using System.Security.Claims;

namespace Altinn.Authorization.TestUtils.AspNetCore.Authentication;

internal sealed record JsonClaim(string Type, string Value, string? ValueType, string? Issuer)
{
    public static JsonClaim FromClaim(Claim claim) => new(claim.Type, claim.Value, claim.ValueType, claim.Issuer);
    public Claim ToClaim() => new(type: Type, value: Value, valueType: ValueType, issuer: Issuer);
}
