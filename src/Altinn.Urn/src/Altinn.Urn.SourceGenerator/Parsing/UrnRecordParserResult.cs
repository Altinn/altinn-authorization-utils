using Altinn.Urn.SourceGenerator.Utils;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal readonly record struct UrnRecordParserResult
{
    public required EquitableArray<DiagnosticInfo> Diagnostics { get; init; }

    public string? JsonConverterAttribute { get; init; }

    public string? JsonConverterConcreteType { get; init; }

    public UrnRecordInfo RecordInfo { get; init; }

    public bool IsDefault => Diagnostics.IsDefault;

    [MemberNotNullWhen(true, nameof(JsonConverterAttribute))]
    [MemberNotNullWhen(true, nameof(JsonConverterConcreteType))]
    public bool HasRecordInfo 
        => !IsDefault
        && JsonConverterAttribute is not null
        && JsonConverterConcreteType is not null;
}
