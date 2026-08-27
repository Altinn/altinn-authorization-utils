using System.Collections.Concurrent;
using System.Net;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Standard problem descriptors for HTTP status codes.
/// </summary>
/// <remarks>
/// These should only be used for problems where we have no more specific descriptor, and should
/// <strong>seldom</strong> be used by application code.
/// </remarks>
public static class HttpProblemDescriptors
{
    internal const string DOMAIN_NAME = "HTTP";

    private static readonly ProblemDescriptorFactory _factory
        = ProblemDescriptorFactory.New(DOMAIN_NAME);

    private static readonly ConcurrentDictionary<HttpStatusCode, ProblemDescriptor> _descriptors
        = new();

    /// <summary>
    /// Gets a problem descriptor for a given HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A <see cref="ProblemDescriptor"/> for the given HTTP status code.</returns>
    public static ProblemDescriptor For(HttpStatusCode statusCode)
        => _descriptors.GetOrAdd(
            statusCode,
            static code => _factory.Create((uint)(int)code, code, StatusDescriptions.GetStatusDescription((int)code) ?? $"HTTP {(int)code}"));
}
