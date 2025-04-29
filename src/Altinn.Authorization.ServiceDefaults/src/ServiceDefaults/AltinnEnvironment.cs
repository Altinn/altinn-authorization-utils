using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults;

/// <summary>
/// An Altinn environment descriptor.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record AltinnEnvironment
    : IFormattable
{
    private static readonly AltinnEnvironment LocalDev = new AltinnEnvironment("LOCAL", AltinnEnvironmentType.LocalDev);

    private static readonly AltinnEnvironment AT21 = new AltinnEnvironment("AT21", AltinnEnvironmentType.AT);

    private static readonly AltinnEnvironment AT22 = new AltinnEnvironment("AT22", AltinnEnvironmentType.AT);

    private static readonly AltinnEnvironment AT23 = new AltinnEnvironment("AT23", AltinnEnvironmentType.AT);

    private static readonly AltinnEnvironment AT24 = new AltinnEnvironment("AT24", AltinnEnvironmentType.AT);

    private static readonly AltinnEnvironment YT01 = new AltinnEnvironment("YT01", AltinnEnvironmentType.YT);

    private static readonly AltinnEnvironment TT02 = new AltinnEnvironment("TT02", AltinnEnvironmentType.TT);

    private static readonly AltinnEnvironment PROD = new AltinnEnvironment("PROD", AltinnEnvironmentType.PROD);

    internal static AltinnEnvironment Create(string name)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        return name.ToUpperInvariant() switch 
        {
            "LOCAL" => LocalDev,
            "AT21" => AT21,
            "AT22" => AT22,
            "AT23" => AT23,
            "AT24" => AT24,
            "YT01" => YT01,
            "TT02" => TT02,
            "PROD" => PROD,
            var upper => new AltinnEnvironment(upper, AltinnEnvironmentType.Unknown),
        };
    }

    private readonly string _name;
    private readonly string _nameLower;
    private readonly AltinnEnvironmentType _type;

    private AltinnEnvironment(string name, AltinnEnvironmentType type)
    {
        _name = name;
        _nameLower = name.ToLowerInvariant();
        _type = type;
    }

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

    /// <inheritdoc/>
    public override string ToString()
        => _name;

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
        => format switch
        {
            "l" => _nameLower,
            null or "" or "U" => _name,
            _ => ThrowHelper.ThrowFormatException<string>($"The format '{format}' is not supported."),
        };

    private enum AltinnEnvironmentType
    {
        Unknown,
        LocalDev,
        AT,
        YT,
        TT,
        PROD,
    }
}
