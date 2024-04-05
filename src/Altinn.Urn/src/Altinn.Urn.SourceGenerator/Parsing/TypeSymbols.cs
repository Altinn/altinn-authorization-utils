using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal readonly struct TypeSymbols
{
    public required INamedTypeSymbol UrnAttribute { get; init; }

    public required INamedTypeSymbol UrnTypeAttribute { get; init; }

    public required INamedTypeSymbol IUrn { get; init; }

    public required INamedTypeSymbol IUrnOfT { get; init; }

    public required INamedTypeSymbol IUrnOfTAndVariant { get; init; }

    public required INamedTypeSymbol JsonConverterAttribute { get; init; }

    public required INamedTypeSymbol UrnJsonConverterOfT { get; init; }

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

        if (compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnAttribute, cancellationToken, out var urnAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnTypeAttribute, cancellationToken, out var urnTypeAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterface, cancellationToken, out var iUrn)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterfaceOfT, cancellationToken, out var iUrnOfT)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnInterfaceOfTAndVariant, cancellationToken, out var iUrnOfTAndVariant)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.JsonConverterAttribute, cancellationToken, out var jsonConverterAttribute)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.UrnJsonConverterOfT, cancellationToken, out var urnJsonConverterOfT)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.FormatProviderInterface, cancellationToken, out var iFormatProvider)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.Span, cancellationToken, out var span)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.ReadOnlySpan, cancellationToken, out var readOnlySpan)
            && compilation.TryGetBestTypeByMetadataName(MetadataNames.SpanFormattableInterface, cancellationToken, out var iSpanFormattable))
        {
            var charType = compilation.GetSpecialType(SpecialType.System_Char);

            symbols = new TypeSymbols
            {
                UrnAttribute = urnAttribute,
                UrnTypeAttribute = urnTypeAttribute,
                IUrn = iUrn,
                IUrnOfT = iUrnOfT,
                IUrnOfTAndVariant = iUrnOfTAndVariant,
                JsonConverterAttribute = jsonConverterAttribute,
                UrnJsonConverterOfT = urnJsonConverterOfT,
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
        public static readonly string UrnAttribute = "Altinn.Urn.UrnAttribute";

        public static readonly string UrnTypeAttribute = "Altinn.Urn.UrnTypeAttribute";

        public static readonly string UrnInterface = "Altinn.Urn.IUrn";

        public static readonly string UrnInterfaceOfT = "Altinn.Urn.IUrn`1";

        public static readonly string UrnInterfaceOfTAndVariant = "Altinn.Urn.IUrn`2";

        public static readonly string JsonConverterAttribute = "System.Text.Json.Serialization.JsonConverterAttribute";

        public static readonly string UrnJsonConverterOfT = "Altinn.Urn.Json.UrnJsonConverter`1";

        public static readonly string SpanFormattableInterface = "System.ISpanFormattable";

        public static readonly string FormatProviderInterface = typeof(IFormatProvider).FullName;

        public static readonly string Span = typeof(Span<>).FullName;

        public static readonly string ReadOnlySpan = typeof(ReadOnlySpan<>).FullName;
    }
}
