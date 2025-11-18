namespace Altinn.Swashbuckle.Security;

/// <summary>
/// Represents the context for OpenAPI security providers.
/// </summary>
public sealed record OpenApiSecurityContext
{
    /// <summary>
    /// Gets the name of the document associated with this instance.
    /// </summary>
    public required string DocumentName { get; init; }
}
