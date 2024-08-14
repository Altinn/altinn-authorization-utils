using System.Diagnostics.CodeAnalysis;

namespace Altinn.Swashbuckle.Filters;

/// <summary>
/// Attribute for specifying that a type should be represented as a string in the OpenAPI schema.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
public sealed class SwaggerStringAttribute
    : Attribute
{
    /// <summary>
    /// Gets or sets the (optional) format of the string.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Gets or sets the (optional) pattern of the string.
    /// </summary>
    [StringSyntax(StringSyntaxAttribute.Regex)]
    public string? Pattern { get; set; }
}
