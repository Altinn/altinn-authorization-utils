using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Strategies for choosing the next semantic version.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ReleasePleaseVersioningStrategy>))]
public enum ReleasePleaseVersioningStrategy
{
    /// <summary>Uses Conventional Commits to select a semantic-version bump.</summary>
    [JsonStringEnumMemberName("default")]
    Default,

    /// <summary>Always increments at least the patch version.</summary>
    [JsonStringEnumMemberName("always-bump-patch")]
    AlwaysBumpPatch,

    /// <summary>Always increments at least the minor version.</summary>
    [JsonStringEnumMemberName("always-bump-minor")]
    AlwaysBumpMinor,

    /// <summary>Always increments the major version.</summary>
    [JsonStringEnumMemberName("always-bump-major")]
    AlwaysBumpMajor,

    /// <summary>Uses service-pack versioning.</summary>
    [JsonStringEnumMemberName("service-pack")]
    ServicePack,

    /// <summary>Uses prerelease versioning.</summary>
    [JsonStringEnumMemberName("prerelease")]
    Prerelease,
}
