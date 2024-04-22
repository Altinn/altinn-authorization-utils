using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal readonly struct TypeSymbols
{
    public required INamedTypeSymbol KeyValueUrnAttribute { get; init; }

    public required INamedTypeSymbol UrnKeyAttribute { get; init; }

    public required INamedTypeSymbol IKeyValueUrn { get; init; }

    public required INamedTypeSymbol IKeyValueUrnOfT { get; init; }

    public required INamedTypeSymbol IKeyValueUrnOfTAndVariant { get; init; }

    public required INamedTypeSymbol IKeyValueUrnVariant { get; init; }

    public required INamedTypeSymbol IVisitableKeyValueUrn { get; init; }

    public required INamedTypeSymbol IKeyValueVisitor { get; init; }

    public required INamedTypeSymbol JsonConverterAttribute { get; init; }

    public required INamedTypeSymbol UrnJsonConverterOfT { get; init; }

    public required INamedTypeSymbol UrnVariantJsonConverter { get; init; }

    public required INamedTypeSymbol IFormatProvider { get; init; }

    public required INamedTypeSymbol ISpanFormattable { get; init; }

    public required INamedTypeSymbol Span { get; init; }

    public required INamedTypeSymbol ReadOnlySpan { get; init; }

    public required INamedTypeSymbol SpanOfChar { get; init; }

    public required INamedTypeSymbol ReadOnlySpanOfChar { get; init; }

    public required INamedTypeSymbol Bool { get; init; }

    public required INamedTypeSymbol Int { get; init; }

    public required INamedTypeSymbol String { get; init; }

    public static bool TryCreate(Compilation? compilation, CancellationToken cancellationToken, [NotNullWhen(true)] out TypeSymbols symbols)
    {
        if (compilation is null)
        {
            symbols = default;
            return false;
        }

        if (compilation.TryGetBestTypeByMetadataName(MetadataNames.KeyValueUrnAttribute, cancellationToken, out var urnAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnKeyAttribute, cancellationToken, out var urnTypeAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterface, cancellationToken, out var iUrn)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterfaceOfT, cancellationToken, out var iUrnOfT)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterfaceOfTAndVariant, cancellationToken, out var iUrnOfTAndVariant)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnVariantInterface, cancellationToken, out var iUrnVariant)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.VisitableUrnInterface, cancellationToken, out var iVisitableUrn)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.KeyValueVisitorInterface, cancellationToken, out var iKeyValueVisitor)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.JsonConverterAttribute, cancellationToken, out var jsonConverterAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnJsonConverterOfT, cancellationToken, out var urnJsonConverterOfT)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnVariantJsonConverter, cancellationToken, out var urnVariantJsonConverter)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.FormatProviderInterface, cancellationToken, out var iFormatProvider)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.Span, cancellationToken, out var span)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.ReadOnlySpan, cancellationToken, out var readOnlySpan)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.SpanFormattableInterface, cancellationToken, out var iSpanFormattable))
        {
            var charType = compilation.GetSpecialType(SpecialType.System_Char);

            symbols = new TypeSymbols
            {
                KeyValueUrnAttribute = urnAttribute,
                UrnKeyAttribute = urnTypeAttribute,
                IKeyValueUrn = iUrn,
                IKeyValueUrnOfT = iUrnOfT,
                IKeyValueUrnOfTAndVariant = iUrnOfTAndVariant,
                IKeyValueUrnVariant = iUrnVariant,
                IVisitableKeyValueUrn = iVisitableUrn,
                IKeyValueVisitor = iKeyValueVisitor,
                JsonConverterAttribute = jsonConverterAttribute,
                UrnJsonConverterOfT = urnJsonConverterOfT,
                UrnVariantJsonConverter = urnVariantJsonConverter,
                IFormatProvider = iFormatProvider,
                ISpanFormattable = iSpanFormattable,
                Span = span,
                ReadOnlySpan = readOnlySpan,
                SpanOfChar = span.Construct(charType),
                ReadOnlySpanOfChar = readOnlySpan.Construct(charType),
                Bool = compilation.GetSpecialType(SpecialType.System_Boolean),
                Int = compilation.GetSpecialType(SpecialType.System_Int32),
                String = compilation.GetSpecialType(SpecialType.System_String),
            };
            return true;
        }

        symbols = default;
        return false;
    }

    public static class MetadataNames
    {
        public static readonly string KeyValueUrnAttribute = "Altinn.Urn.KeyValueUrnAttribute";

        public static readonly string UrnKeyAttribute = "Altinn.Urn.UrnKeyAttribute";

        public static readonly string UrnInterface = "Altinn.Urn.IKeyValueUrn";

        public static readonly string UrnInterfaceOfT = "Altinn.Urn.IKeyValueUrn`1";

        public static readonly string UrnInterfaceOfTAndVariant = "Altinn.Urn.IKeyValueUrn`2";

        public static readonly string UrnVariantInterface = "Altinn.Urn.IKeyValueUrnVariant`4";

        public static readonly string VisitableUrnInterface = "Altinn.Urn.Visit.IVisitableKeyValueUrn";

        public static readonly string KeyValueVisitorInterface = "Altinn.Urn.Visit.IKeyValueUrnVisitor";

        public static readonly string JsonConverterAttribute = "System.Text.Json.Serialization.JsonConverterAttribute";

        public static readonly string UrnJsonConverterOfT = "Altinn.Urn.Json.UrnJsonConverter`1";

        public static readonly string UrnVariantJsonConverter = "Altinn.Urn.Json.UrnVariantJsonConverterFactory`2";

        public static readonly string SpanFormattableInterface = "System.ISpanFormattable";

        public static readonly string FormatProviderInterface = typeof(IFormatProvider).FullName;

        public static readonly string Span = typeof(Span<>).FullName;

        public static readonly string ReadOnlySpan = typeof(ReadOnlySpan<>).FullName;
    }
}
