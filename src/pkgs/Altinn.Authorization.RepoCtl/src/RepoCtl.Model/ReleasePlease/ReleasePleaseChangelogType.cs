using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Changelog notes providers supported by release-please.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<ReleasePleaseChangelogType>))]
public enum ReleasePleaseChangelogType
{
    /// <summary>Uses release-please's default changelog notes provider.</summary>
    [JsonStringEnumMemberName("default")]
    Default,

    /// <summary>Uses GitHub-generated release notes.</summary>
    [JsonStringEnumMemberName("github")]
    GitHub,
}
