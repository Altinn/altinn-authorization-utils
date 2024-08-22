using Altinn.Swashbuckle.Examples;

namespace Altinn.Swashbuckle.Filters;

/// <summary>
/// Attribute for specifying that a type should have example generated using the <see cref="IExampleDataProvider{TSelf}"/> machinery.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
public sealed class SwaggerExampleFromExampleProviderAttribute
    : Attribute
{
}
