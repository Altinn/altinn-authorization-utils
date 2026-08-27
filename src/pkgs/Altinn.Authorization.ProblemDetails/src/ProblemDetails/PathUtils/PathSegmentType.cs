namespace Altinn.Authorization.ProblemDetails.PathUtils;

/// <summary>
/// Defines the type of a path segment.
/// </summary>
public enum PathSegmentType
{
    /// <summary>
    /// The segment is a property name.
    /// </summary>
    Property = 1,

    /// <summary>
    /// The segment is an array indexer.
    /// </summary>
    Indexer,
}
