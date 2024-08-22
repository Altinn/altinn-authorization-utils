using Altinn.Swashbuckle.Examples;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

/// <summary>
/// Attribute for specifying that a type should have example generated using the <see cref="IExampleDataProvider{TSelf}"/> machinery.
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
public sealed class SwaggerExampleFromExampleProviderAttribute
    : Attribute
{
}
