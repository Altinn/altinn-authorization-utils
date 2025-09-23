using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// An Altinn environment descriptor.
/// </summary>
public sealed record AltinnEnvironment
    : IFormattable
{
    private static readonly AltinnEnvironment LocalAT21 = new("LOCAL-AT21", AltinnEnvironmentType.LocalDev, AltinnEnvironmentId.AT21);

    private static readonly AltinnEnvironment LocalAT22 = new("LOCAL-AT22", AltinnEnvironmentType.LocalDev, AltinnEnvironmentId.AT22);

    private static readonly AltinnEnvironment LocalAT23 = new("LOCAL-AT23", AltinnEnvironmentType.LocalDev, AltinnEnvironmentId.AT23);

    private static readonly AltinnEnvironment LocalAT24 = new("LOCAL-AT24", AltinnEnvironmentType.LocalDev, AltinnEnvironmentId.AT24);

    private static readonly AltinnEnvironment AT21 = new("AT21", AltinnEnvironmentType.AT, AltinnEnvironmentId.AT21);

    private static readonly AltinnEnvironment AT22 = new("AT22", AltinnEnvironmentType.AT, AltinnEnvironmentId.AT22);

    private static readonly AltinnEnvironment AT23 = new("AT23", AltinnEnvironmentType.AT, AltinnEnvironmentId.AT23);

    private static readonly AltinnEnvironment AT24 = new("AT24", AltinnEnvironmentType.AT, AltinnEnvironmentId.AT24);

    private static readonly AltinnEnvironment YT01 = new("YT01", AltinnEnvironmentType.YT, AltinnEnvironmentId.YT01);

    private static readonly AltinnEnvironment TT02 = new("TT02", AltinnEnvironmentType.TT, AltinnEnvironmentId.TT02);

    private static readonly AltinnEnvironment PROD = new("PROD", AltinnEnvironmentType.PROD, AltinnEnvironmentId.PROD);

    internal static AltinnEnvironment Create(string name)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        return name.ToUpperInvariant() switch 
        {
            "LOCAL" => LocalAT22, // backward compatibility
            "LOCAL-AT21" => LocalAT21,
            "LOCAL-AT22" => LocalAT22,
            "LOCAL-AT23" => LocalAT23,
            "LOCAL-AT24" => LocalAT24,
            "AT21" => AT21,
            "AT22" => AT22,
            "AT23" => AT23,
            "AT24" => AT24,
            "YT01" => YT01,
            "TT02" => TT02,
            "PROD" => PROD,
            var upper => new AltinnEnvironment(upper, AltinnEnvironmentType.Unknown, AltinnEnvironmentId.Unknown),
        };
    }

    private readonly string _name;
    private readonly string _nameLower;
    private readonly string _platformEnvName;
    private readonly string _platformEnvNameLower;
    private readonly AltinnEnvironmentType _type;
    private readonly AltinnEnvironmentId _id;

    private AltinnEnvironment(string name, AltinnEnvironmentType type, AltinnEnvironmentId id)
    {
        _id = id;
        _name = name;
        _nameLower = name.ToLowerInvariant();
        _type = type;

        if (type != AltinnEnvironmentType.LocalDev)
        {
            if (name.StartsWith("LOCAL-", StringComparison.Ordinal))
            {
                ThrowHelper.ThrowArgumentException(nameof(name), "Non-known environment names cannot start with 'LOCAL-'.");
            }

            _platformEnvName = name;
            _platformEnvNameLower = _nameLower;
        }
        else
        {
            Debug.Assert(name.StartsWith("LOCAL-", StringComparison.OrdinalIgnoreCase));
            _platformEnvName = name["LOCAL-".Length..];
            _platformEnvNameLower = _platformEnvName.ToLowerInvariant();
        }
    }

    /// <summary>
    /// Gets the environment id.
    /// </summary>
    public AltinnEnvironmentId Id => _id;

    /// <summary>
    /// Gets a value indicating whether the environment is local-dev.
    /// </summary>
    public bool IsLocalDev => _type == AltinnEnvironmentType.LocalDev;

    /// <summary>
    /// Gets a value indicating whether the environment is an AT environment.
    /// </summary>
    public bool IsAT => _type == AltinnEnvironmentType.AT;

    /// <summary>
    /// Gets a value indicating whether the environment is a YT environment.
    /// </summary>
    public bool IsYT => _type == AltinnEnvironmentType.YT;

    /// <summary>
    /// Gets a value indicating whether the environment is a TT environment.
    /// </summary>
    public bool IsTT => _type == AltinnEnvironmentType.TT;

    /// <summary>
    /// Gets a value indicating whether the environment is a PROD environment.
    /// </summary>
    public bool IsProd => _type == AltinnEnvironmentType.PROD;

    /// <summary>
    /// Gets a value indicating whether the environment is unknown or a unrecognized environment.
    /// </summary>
    public bool IsUnknown => _type == AltinnEnvironmentType.Unknown;

    /// <inheritdoc/>
    public override string ToString()
        => _name;

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
        => format switch
        {
            "P" => _platformEnvName,
            "p" => _platformEnvNameLower,
            "l" => _nameLower,
            null or "" or "U" => _name,
            _ => ThrowHelper.ThrowFormatException<string>($"The format '{format}' is not supported."),
        };

    private enum AltinnEnvironmentType
        : byte
    {
        Unknown,
        LocalDev,
        AT,
        YT,
        TT,
        PROD,
    }
}
