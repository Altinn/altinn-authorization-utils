using Altinn.Urn.SourceGenerator.Utils;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal readonly record struct UrnRecordInfo
{
    public required string Keyword { get; init; }

    public required string Namespace { get; init; }

    public required EquitableArray<ContainingType> ContainingTypes { get; init; }

    public required string TypeName { get; init; }

    public required EquitableArray<UrnPrefixInfo> Members { get; init; }
}
