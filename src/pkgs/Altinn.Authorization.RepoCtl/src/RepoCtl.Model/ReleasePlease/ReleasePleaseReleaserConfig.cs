using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Options shared by a release-please manifest and its individual packages.
/// </summary>
public abstract record ReleasePleaseReleaserConfig
{
    /// <summary>
    /// Gets the release strategy used to discover and update the package's version files.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ReleaseType)]
    [JsonPropertyName("release-type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ReleasePleaseReleaseType ReleaseType { get; set; }

    /// <summary>
    /// Gets whether breaking changes bump the minor version instead of the major version while the current major version is zero.
    /// The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.BumpMinorPreMajor)]
    [JsonPropertyName("bump-minor-pre-major")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? BumpMinorPreMajor { get; set; }

    /// <summary>
    /// Gets whether features bump the patch version instead of the minor version while the current major version is zero.
    /// The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.BumpPatchForMinorPreMajor)]
    [JsonPropertyName("bump-patch-for-minor-pre-major")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? BumpPatchForMinorPreMajor { get; set; }

    /// <summary>
    /// Gets the prerelease identifier, such as <c>beta</c>, used when constructing prerelease versions.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.PrereleaseType)]
    [JsonPropertyName("prerelease-type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PrereleaseType { get; set; }

    /// <summary>
    /// Gets the strategy used to determine the next semantic version. The default is <see cref="ReleasePleaseVersioningStrategy.Default"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Versioning)]
    [JsonPropertyName("versioning")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ReleasePleaseVersioningStrategy Versioning { get; set; }

    /// <summary>
    /// Gets the mapping from Conventional Commit types to changelog sections.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ChangelogSections)]
    [JsonPropertyName("changelog-sections")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ReleasePleaseChangelogSection>? ChangelogSections { get; set; }

    /// <summary>
    /// Gets a version to use for the next release regardless of the commits found.
    /// </summary>
    /// <remarks>This deprecated option is equivalent to a <c>Release-As</c> commit footer.</remarks>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ReleaseAs)]
    [JsonPropertyName("release-as")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [Obsolete("Use a 'Release-As' commit footer instead.")]
    public string? ReleaseAs { get; set; }

    /// <summary>
    /// Gets whether creation of the GitHub release is skipped. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SkipGitHubRelease)]
    [JsonPropertyName("skip-github-release")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SkipGitHubRelease { get; set; }

    /// <summary>
    /// Gets whether changelog updates are skipped. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SkipChangelog)]
    [JsonPropertyName("skip-changelog")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SkipChangelog { get; set; }

    /// <summary>
    /// Gets whether the GitHub release is created as a draft. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Draft)]
    [JsonPropertyName("draft")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Draft { get; set; }

    /// <summary>
    /// Gets whether a tag is created immediately for a draft release. The default is <see langword="false"/>.
    /// </summary>
    /// <remarks>Without this option, release-please creates the tag only when the draft release is published.</remarks>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ForceTagCreation)]
    [JsonPropertyName("force-tag-creation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ForceTagCreation { get; set; }

    /// <summary>
    /// Gets whether the GitHub release is marked as a prerelease. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Prerelease)]
    [JsonPropertyName("prerelease")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Prerelease { get; set; }

    /// <summary>
    /// Gets whether the release pull request is created as a draft. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.DraftPullRequest)]
    [JsonPropertyName("draft-pull-request")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? DraftPullRequest { get; set; }

    /// <summary>
    /// Gets additional comma-separated labels applied to release pull requests.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ExtraLabel)]
    [JsonPropertyName("extra-label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExtraLabel { get; set; }

    /// <summary>
    /// Gets whether the component is included in the release tag. The default is <see langword="true"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.IncludeComponentInTag)]
    [JsonPropertyName("include-component-in-tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeComponentInTag { get; set; }

    /// <summary>
    /// Gets whether the version in the release tag is prefixed with <c>v</c>. The default is <see langword="true"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.IncludeVInTag)]
    [JsonPropertyName("include-v-in-tag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeVInTag { get; set; }

    /// <summary>
    /// Gets whether the version in the GitHub release name is prefixed with <c>v</c>. The default is <see langword="true"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.IncludeVInReleaseName)]
    [JsonPropertyName("include-v-in-release-name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeVInReleaseName { get; set; }

    /// <summary>
    /// Gets the changelog notes provider. The default is <see cref="ReleasePleaseChangelogType.Default"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ChangelogType)]
    [JsonPropertyName("changelog-type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ReleasePleaseChangelogType ChangelogType { get; set; }

    /// <summary>
    /// Gets the host used to generate commit and pull-request links in changelog entries.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ChangelogHost)]
    [JsonPropertyName("changelog-host")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ChangelogHost { get; set; }

    /// <summary>
    /// Gets whether changelog entries include the commit author's name or GitHub username.
    /// The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.IncludeCommitAuthors)]
    [JsonPropertyName("include-commit-authors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeCommitAuthors { get; set; }

    /// <summary>
    /// Gets the changelog file path relative to the package. The default is <c>CHANGELOG.md</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ChangelogPath)]
    [JsonPropertyName("changelog-path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ChangelogPath { get; set; }

    /// <summary>
    /// Gets the template used for release pull-request titles.
    /// </summary>
    /// <remarks>The template supports the <c>${component}</c>, <c>${version}</c>, and <c>${branch}</c> placeholders.</remarks>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.PullRequestTitlePattern)]
    [JsonPropertyName("pull-request-title-pattern")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PullRequestTitlePattern { get; set; }

    /// <summary>
    /// Gets text prepended to the release pull-request body.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.PullRequestHeader)]
    [JsonPropertyName("pull-request-header")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PullRequestHeader { get; set; }

    /// <summary>
    /// Gets text appended to the release pull-request body.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.PullRequestFooter)]
    [JsonPropertyName("pull-request-footer")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PullRequestFooter { get; set; }

    /// <summary>
    /// Gets whether each package receives a separate release pull request. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SeparatePullRequests)]
    [JsonPropertyName("separate-pull-requests")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SeparatePullRequests { get; set; }

    /// <summary>
    /// Gets the separator placed between the component and version in a release tag. The default is <c>-</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.TagSeparator)]
    [JsonPropertyName("tag-separator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TagSeparator { get; set; }

    /// <summary>
    /// Gets the <c>strftime</c>-style format used for dates in changelog entries.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.DateFormat)]
    [JsonPropertyName("date-format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DateFormat { get; set; }

    /// <summary>
    /// Gets additional files whose version references release-please updates.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ExtraFiles)]
    [JsonPropertyName("extra-files")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ReleasePleaseExtraFile>? ExtraFiles { get; set; }

    /// <summary>
    /// Gets paths or glob patterns excluded from commit processing.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ExcludePaths)]
    [JsonPropertyName("exclude-paths")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ExcludePaths { get; set; }

    /// <summary>
    /// Gets the version file used by the <c>ruby</c> or <c>simple</c> release strategy.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.VersionFile)]
    [JsonPropertyName("version-file")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VersionFile { get; set; }

    /// <summary>
    /// Gets the label that triggers snapshot version updates for Java strategies.
    /// The default is <c>autorelease: snapshot</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SnapshotLabel)]
    [JsonPropertyName("snapshot-label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SnapshotLabel { get; set; }

    /// <summary>
    /// Gets the comma-separated labels applied to release pull requests. The default is <c>autorelease: pending</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Label)]
    [JsonPropertyName("label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Label { get; set; }

    /// <summary>
    /// Gets the comma-separated labels applied after a pull request has been released. The default is <c>autorelease: tagged</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ReleaseLabel)]
    [JsonPropertyName("release-label")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReleaseLabel { get; set; }

    /// <summary>
    /// Gets whether automatic snapshot version updates are skipped for Java strategies. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SkipSnapshot)]
    [JsonPropertyName("skip-snapshot")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SkipSnapshot { get; set; }

    /// <summary>
    /// Gets the version used when a package has not previously been released. The default is <c>1.0.0</c>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.InitialVersion)]
    [JsonPropertyName("initial-version")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InitialVersion { get; set; }

    /// <summary>
    /// Gets whether a space between the component and version is omitted in release pull-request titles.
    /// The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ComponentNoSpace)]
    [JsonPropertyName("component-no-space")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ComponentNoSpace { get; set; }
}
