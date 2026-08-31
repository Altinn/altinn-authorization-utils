using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Maps a Conventional Commit type to a changelog section.
/// </summary>
public sealed record ReleasePleaseChangelogSection
{
    /// <summary>Gets the Conventional Commit type, such as <c>feat</c> or <c>fix</c>.</summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    /// <summary>Gets the heading used for commits of this type in the changelog.</summary>
    [JsonPropertyName("section")]
    public required string Section { get; set; }

    /// <summary>Gets whether commits of this type are omitted from the changelog. The default is <see langword="false"/>.</summary>
    [JsonPropertyName("hidden")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Hidden { get; set; }
}
