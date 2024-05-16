using System.Net;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A factory for creating <see cref="AltinnProblemDetails"/>.
/// </summary>
public sealed class AltinnProblemDetailsFactory
{
    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetailsFactory"/> for a given domain name.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns>A <see cref="AltinnProblemDetailsFactory"/>.</returns>
    /// <remarks>Domain names must be 2-4 letter ASCII uppercase.</remarks>
    public static AltinnProblemDetailsFactory New(string domainName)
        => new(ErrorCodeDomain.Get(domainName));

    private readonly ErrorCodeDomain _domain;

    private AltinnProblemDetailsFactory(ErrorCodeDomain domain)
    {
        _domain = domain;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the error.</param>
    /// <param name="detail">The error details (message).</param>
    /// <returns>A newly created <see cref="AltinnProblemDetails"/>.</returns>
    public AltinnProblemDetails Create(uint code, HttpStatusCode statusCode, string detail)
        => new AltinnProblemDetails(_domain.Code(code), statusCode, detail);

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the error.</param>
    /// <param name="detail">The error details (message).</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A newly created <see cref="AltinnProblemDetails"/>.</returns>
    public AltinnProblemDetails Create(uint code, HttpStatusCode statusCode, string detail, ReadOnlySpan<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnProblemDetails(_domain.Code(code), statusCode, detail);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }

    /// <summary>
    /// Creates a new <see cref="AltinnProblemDetails"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="statusCode">The <see cref="HttpStatusCode"/> for the error.</param>
    /// <param name="detail">The error details (message).</param>
    /// <param name="extensions">Additional properties for the error.</param>
    /// <returns>A newly created <see cref="AltinnProblemDetails"/>.</returns>
    public AltinnProblemDetails Create(uint code, HttpStatusCode statusCode, string detail, IEnumerable<KeyValuePair<string, object?>> extensions)
    {
        var ret = new AltinnProblemDetails(_domain.Code(code), statusCode, detail);
        foreach (var (key, value) in extensions)
        {
            ret.Extensions.Add(key, value);
        }

        return ret;
    }
}
