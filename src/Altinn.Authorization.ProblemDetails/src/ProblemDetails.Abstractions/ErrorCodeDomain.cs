using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Represents a domain for <see cref="ErrorCode"/>s.
/// </summary>
[DebuggerDisplay("Domain = {Name}")]
internal sealed class ErrorCodeDomain
{
    internal const int ROOT_MIN_LENGTH = 2;
    internal const int ROOT_MAX_LENGTH = 4;

    internal const int SUB_MIN_LENGTH = 1;
    internal const int SUB_MAX_LENGTH = 4;

    internal const int MIN_LENGTH = ROOT_MIN_LENGTH;
    internal const int MAX_LENGTH = ROOT_MAX_LENGTH + SUB_MAX_LENGTH + 1;
    
    private static readonly SearchValues<char> VALID_CHARS
        = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    private static ImmutableDictionary<string, ErrorCodeDomain> _domains
        = ImmutableDictionary<string, ErrorCodeDomain>.Empty;

    /// <summary>
    /// Gets a <see cref="ErrorCodeDomain"/> for a given domain name.
    /// </summary>
    /// <param name="name">The domain name.</param>
    /// <returns>A singleton <see cref="ErrorCodeDomain"/>.</returns>
    /// <remarks>Domain names must be 2-4 letter ASCII uppercase.</remarks>
    public static ErrorCodeDomain Get(string name)
        => ImmutableInterlocked.GetOrAdd(ref _domains, name, Create);

    private static ErrorCodeDomain Create(string name)
        => new ErrorCodeDomain(name);

    private readonly string _name;
    private readonly ErrorCodeDomain _root;

    private ImmutableDictionary<string, ErrorCodeDomain> _subDomains
        = ImmutableDictionary<string, ErrorCodeDomain>.Empty;

    private ErrorCodeDomain(string name)
    {
        Guard.IsNotNullOrWhiteSpace(name);
        Guard.HasSizeLessThanOrEqualTo(name, MAX_LENGTH);
        Guard.HasSizeGreaterThanOrEqualTo(name, MIN_LENGTH);
        
        if (name.AsSpan().ContainsAnyExcept(VALID_CHARS))
        {
            ThrowHelper.ThrowArgumentException(nameof(name), "Domain name must be uppercase ASCII letters only.");
        }

        _name = name;
        _root = this;
    }

    private ErrorCodeDomain(string name, ErrorCodeDomain root)
    {
        Guard.IsNotNullOrWhiteSpace(name);
        Guard.HasSizeLessThanOrEqualTo(name, SUB_MAX_LENGTH);
        Guard.HasSizeGreaterThanOrEqualTo(name, SUB_MIN_LENGTH);

        if (name.AsSpan().ContainsAnyExcept(VALID_CHARS))
        {
            ThrowHelper.ThrowArgumentException(nameof(name), "Domain name must be uppercase ASCII letters only.");
        }

        _name = $"{root.Name}.{name}";
        _root = root;
    }

    internal string Name => _name;

    /// <summary>
    /// Gets a subdomain of this domain.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    internal ErrorCodeDomain SubDomain(string name)
    {
        if (!ReferenceEquals(this, _root))
        {
            ThrowHelper.ThrowInvalidOperationException("Subdomains can only be created from root domains.");
        }

        return ImmutableInterlocked.GetOrAdd(ref _subDomains, name, CreateSubDomain);
    }

    private ErrorCodeDomain CreateSubDomain(string name)
        => new ErrorCodeDomain(name, this);

    /// <summary>
    /// Creates a new <see cref="ErrorCode"/> for this domain.
    /// </summary>
    /// <param name="code">The code value.</param>
    /// <returns>The created <see cref="ErrorCode"/>.</returns>
    public ErrorCode Code(uint code)
        => new ErrorCode(this, code);
}
