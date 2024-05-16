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
    internal const int MIN_LENGTH = 2;
    internal const int MAX_LENGTH = 4;
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
    }

    internal string Name => _name;

    /// <summary>
    /// Creates a new <see cref="ErrorCode"/> for this domain.
    /// </summary>
    /// <param name="code">The code value.</param>
    /// <returns>The created <see cref="ErrorCode"/>.</returns>
    public ErrorCode Code(uint code)
        => new ErrorCode(this, code);
}
