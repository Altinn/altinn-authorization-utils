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
    private readonly ErrorCodeTracking _tracking = new();

    private ProblemDescriptorFactory(ErrorCodeDomain domain)
    {
        _domain = domain;
    }

    /// <summary>
    /// Creates a new <see cref="ProblemDescriptor"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the error.</param>
    /// <param name="title">The error title.</param>
    /// <returns>A newly created <see cref="ProblemDescriptor"/>.</returns>
    /// <remarks>
    /// It is <strong>strongly</strong> encouraged to cache and reuse instances of
    /// <see cref="ProblemDescriptor"/>s created by this method. This is
    /// enforced during debug builds by throwing an exception if the same code
    /// is used more than once.
    /// </remarks>
    public ProblemDescriptor Create(uint code, HttpStatusCode statusCode, string title)
    {
        var errorCode = _domain.Code(code);
        _tracking.Track(code, errorCode);

        return new ProblemDescriptor(errorCode, statusCode, title);
    }
}
