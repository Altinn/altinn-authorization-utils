using Altinn.Urn.Swashbuckle.Tests.Fixtures;

namespace Altinn.Urn.Swashbuckle.Tests;

[KeyValueUrn]
public abstract partial record PersonUrn
{
    [UrnKey("altinn:person:identifier-no")]
    public partial bool IsIdentifierNo(out PersonIdentifier personId);

    [UrnKey("altinn:organization:org-no")]
    public partial bool IsOrganizationNo(out OrgNo orgNo);

    [UrnKey("altinn:party:id")]
    public partial bool IsPartyId(out int partyId);

    [UrnKey("altinn:party:uuid")]
    public partial bool IsPartyUuid(out Guid partyUuid);

    [UrnKey("altinn:person:d-number")]
    public partial bool IsDNumber(out int dNumber);
}
