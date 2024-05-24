using System.Net;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A factory for creating <see cref="ProblemDescriptor"/>s.
/// </summary>
public sealed class ProblemDescriptorFactory
{
    /// <summary>
    /// Creates a new <see cref="ProblemDescriptorFactory"/> for a given domain name.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns>A <see cref="ProblemDescriptorFactory"/>.</returns>
    /// <remarks>Domain names must be 2-4 letter ASCII uppercase.</remarks>
    public static ProblemDescriptorFactory New(string domainName)
        => new(ErrorCodeDomain.Get(domainName));

    private readonly ErrorCodeDomain _domain;

    private ProblemDescriptorFactory(ErrorCodeDomain domain)
    {
        _domain = domain;
    }

    /// <summary>
    /// Creates a new <see cref="ProblemDescriptor"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the error.</param>
    /// <param name="detail">The error details (message).</param>
    /// <returns>A newly created <see cref="ProblemDescriptor"/>.</returns>
    public ProblemDescriptor Create(uint code, HttpStatusCode statusCode, string detail)
        => new ProblemDescriptor(_domain.Code(code), statusCode, detail);
}
