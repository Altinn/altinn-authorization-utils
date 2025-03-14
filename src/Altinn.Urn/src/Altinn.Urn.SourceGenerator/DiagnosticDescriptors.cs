using Microsoft.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator;

/// <summary>
/// <see cref="DiagnosticDescriptor"/>s for the Urn source generator.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string DiagnosticCategory = "UrnGenerator";

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodMustBePartial { get; } = new(
        id: "AURN0001",
        title: "Urn type-methods must be partial",
        messageFormat: "Urn type-methods must be partial",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodHasBody { get; } = new(
        id: "AURN0002",
        title: "Urn type-methods must not have a body",
        messageFormat: "Urn type-methods must not have a body",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodIsGeneric { get; } = new(
        id: "AURN0003",
        title: "Urn type-methods must not be generic",
        messageFormat: "Urn type-methods must not be generic",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodIsStatic { get; } = new(
        id: "AURN0004",
        title: "Urn type-methods must not be static",
        messageFormat: "Urn type-methods must not be static",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixIsEmpty { get; } = new(
        id: "AURN0005",
        title: "Urn type-method prefix is empty",
        messageFormat: "Urn type-method prefix is empty",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixStartsWithUrn { get; } = new(
        id: "AURN0006",
        title: "Urn type-method prefix starts with 'urn'",
        messageFormat: "Urn type-method prefix starts with 'urn'",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixIsDuplicate { get; } = new(
        id: "AURN0007",
        title: "Urn type-method prefix is duplicate",
        messageFormat: "Urn type-method prefix is duplicate",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixHasWhitespace { get; } = new(
        id: "AURN0008",
        title: "Urn type-method prefix has whitespace",
        messageFormat: "Urn type-method prefix has whitespace",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodMustReturnBool { get; } = new(
        id: "AURN0009",
        title: "Urn type-method must return bool",
        messageFormat: "Urn type-method must return bool",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodMustHaveOneParameter { get; } = new(
        id: "AURN0010",
        title: "Urn type-method must have one parameter",
        messageFormat: "Urn type-method must have one parameter",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodParameterMustBeOut { get; } = new(
        id: "AURN0011",
        title: "Urn type-method parameter must be out",
        messageFormat: "Urn type-method parameter must be out",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodMustStartWithIs { get; } = new(
        id: "AURN0012",
        title: "Urn type-method must start with 'Is'",
        messageFormat: "Urn type-method must start with 'Is'",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnRecordHasNoUrnTypeMethods { get; } = new(
        id: "AURN0013",
        title: "Urn record has no Urn type-methods",
        messageFormat: "Urn record has no Urn type-methods",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnRecordIsValueType { get; } = new(
        id: "AURN0014",
        title: "Urn record is a value type",
        messageFormat: "Urn record is a value type",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnRecordMustBeAbstract { get; } = new(
        id: "AURN0015",
        title: "Urn record must be abstract",
        messageFormat: "Urn record must be abstract",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixIsMissingCanonical { get; } = new(
        id: "AURN0016",
        title: "Urn type-method prefix is missing canonical",
        messageFormat: "Urn type-method prefix is missing canonical. This is required because it supports multiple urn prefixes or has explicitly set Canonical to false on all cases.",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixHasMultipleCanonical { get; } = new(
        id: "AURN0017",
        title: "Urn type-method prefix has multiple canonical prefixes",
        messageFormat: "Urn type-method prefix has multiple canonical prefixes",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodHasTryFormatButNoFormat { get; } = new(
        id: "AURN0018",
        title: "Urn type-method has TryFormat but no Format",
        messageFormat: "Urn type-method has TryFormat but no Format",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>Gets the diagnostic descriptor.</summary>
    public static DiagnosticDescriptor UrnTypeMethodPrefixEndsWithColon { get; } = new(
        id: "AURN0019",
        title: "Urn type-method prefix ends with ':'",
        messageFormat: "Urn type-method prefix ends with ':'",
        category: DiagnosticCategory,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
