using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Configuration for one package in a release-please manifest.
/// </summary>
public sealed record ReleasePleasePackage
    : ReleasePleaseReleaserConfig
{
    /// <summary>
    /// Gets the package name used by strategies that require an ecosystem-specific package name.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.PackageName)]
    [JsonPropertyName("package-name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PackageName { get; set; }

    /// <summary>
    /// Gets the component used in release pull-request titles, tags, and release names.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Component)]
    [JsonPropertyName("component")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Component { get; set; }
}
