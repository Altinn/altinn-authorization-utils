using Altinn.Urn.SourceGenerator.Utils;
using Microsoft.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.Parsing;

internal sealed record DiagnosticInfo
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, DiagnosticSeverity? severity, LocationInfo? location, EquitableArray<LocationInfo> additionalLocations = default)
        => new(descriptor, severity, location, additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, LocationInfo? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity: null, location, additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, DiagnosticSeverity? severity, Location? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity, LocationInfo.CreateFrom(location), additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity: null, location, additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, DiagnosticSeverity? severity, SyntaxNode? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity, LocationInfo.CreateFrom(location), additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxNode? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity: null, location, additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, DiagnosticSeverity? severity, SyntaxToken? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity, LocationInfo.CreateFrom(location), additionalLocations);

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, SyntaxToken? location, EquitableArray<LocationInfo> additionalLocations = default)
        => Create(descriptor, severity: null, location, additionalLocations);

    private readonly DiagnosticSeverity? _severity;

    private DiagnosticInfo(DiagnosticDescriptor descriptor, DiagnosticSeverity? severity, LocationInfo? location, EquitableArray<LocationInfo> additionalLocations)
    {
        _severity = severity;

        Descriptor = descriptor;
        Location = location;
        AdditionalLocations = additionalLocations;
    }

    public DiagnosticDescriptor Descriptor { get; }

    public DiagnosticSeverity Severity => _severity ?? Descriptor.DefaultSeverity;

    public LocationInfo? Location { get; }

    public EquitableArray<LocationInfo> AdditionalLocations { get; }
}
