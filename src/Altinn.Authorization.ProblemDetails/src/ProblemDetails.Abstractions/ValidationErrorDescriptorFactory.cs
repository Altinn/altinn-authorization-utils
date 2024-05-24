namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A factory for creating <see cref="ValidationErrorDescriptor"/>s.
/// </summary>
public sealed class ValidationErrorDescriptorFactory
{
    private const string VALIDATION_SUB_DOMAIN_NAME = "VLD";

    /// <summary>
    /// Creates a new <see cref="ValidationErrorDescriptorFactory"/> for a given domain name.
    /// </summary>
    /// <param name="domainName">The domain name.</param>
    /// <returns>A <see cref="ValidationErrorDescriptorFactory"/>.</returns>
    /// <remarks>Domain names must be 2-4 letter ASCII uppercase.</remarks>
    public static ValidationErrorDescriptorFactory New(string domainName)
        => new(ErrorCodeDomain.Get(domainName).SubDomain(VALIDATION_SUB_DOMAIN_NAME));

    private readonly ErrorCodeDomain _domain;

    private ValidationErrorDescriptorFactory(ErrorCodeDomain domain)
    {
        _domain = domain;
    }

    /// <summary>
    /// Creates a new <see cref="ValidationErrorDescriptor"/>.
    /// </summary>
    /// <param name="code">The (domain specific) error code.</param>
    /// <param name="detail">The error details (message).</param>
    /// <returns>A newly created <see cref="ValidationErrorDescriptor"/>.</returns>
    public ValidationErrorDescriptor Create(uint code, string detail)
        => new ValidationErrorDescriptor(_domain.Code(code), detail);
}
