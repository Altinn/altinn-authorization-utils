using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.Sample.Api.Models;

[KeyValueUrn]
[ExcludeFromCodeCoverage]
public abstract partial record PersonUrn
{
    [UrnKey("altinn:person:identifier-no")]
    public partial bool IsIdentifierNo(out PersonIdentifier personId);

    [UrnKey("altinn:organization:org-no", Canonical = true)]
    [UrnKey("altinn:organization:identifier-no")]
    public partial bool IsOrganizationNo(out OrgNo orgNo);

    [UrnKey("altinn:party:id")]
    public partial bool IsPartyId(out int partyId);

    [UrnKey("altinn:party:uuid")]
    public partial bool IsPartyUuid(out Guid partyUuid);

    [UrnKey("altinn:person:d-number")]
    public partial bool IsDNumber(out int dNumber);
}
