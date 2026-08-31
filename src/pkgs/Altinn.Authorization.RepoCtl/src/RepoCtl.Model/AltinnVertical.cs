using System.Collections.Immutable;
using CommunityToolkit.Diagnostics;
using Semver;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents a vertical in an Altinn repository.
/// </summary>
/// <remarks>
/// A vertical is a collection of related projects that are built and released together.
/// </remarks>
public sealed class AltinnVertical
{
    private readonly string _relPath;
    private readonly DirectoryInfo _directory;
    private readonly FileInfo _solutionFile;
    private readonly AltinnVerticalId _id;
    private readonly SemVersion _version;
    private readonly AltinnVerticalConfiguration _config;
    private readonly ImmutableArray<AltinnProject> _projects;
    private readonly string _displayName;
    private AltinnVerticalSet? _directDependencies;
    private AltinnVerticalSet? _directDevDependencies;
    private AltinnVerticalSet? _allDependencies;
    private AltinnVerticalSet? _allBuildDependencies;
    private AltinnVerticalSet? _dependents;

    /// <summary>
    /// Gets the directory of the vertical.
    /// </summary>
    public DirectoryInfo Directory
        => _directory;

    /// <summary>
    /// Gets the solution file of the vertical.
    /// </summary>
    public FileInfo SolutionFile
        => _solutionFile;

    /// <summary>
    /// Gets the relative path of the vertical within the repository.
    /// </summary>
    /// <remarks>
    /// The relative path is relative to the root of the repository,
    /// and uses <c>/</c> as the directory separator.
    /// </remarks>
    public string RelPath
        => _relPath;

    /// <summary>
    /// Gets the identifier of the vertical.
    /// </summary>
    public AltinnVerticalId Id
        => _id;

    /// <summary>
    /// Gets the version of the vertical.
    /// </summary>
    public SemVersion Version
        => _version;

    /// <summary>
    /// Gets the kind of the vertical.
    /// </summary>
    public AltinnVerticalKind Kind
        => _id.Kind;

    /// <summary>
    /// Gets the full name of the vertical.
    /// </summary>
    public string FullName
        => _id.Name;

    /// <summary>
    /// Gets a shortened string representation of the vertical.
    /// </summary>
    public string DisplayId
        => $"{_id.Kind}: {_displayName}";

    /// <summary>
    /// Gets the display name of the vertical.
    /// </summary>
    public string DisplayName
        => _displayName;

    /// <summary>
    /// Gets the configuration of the vertical.
    /// </summary>
    public AltinnVerticalConfiguration Config
        => _config;

    /// <summary>
    /// Gets the projects that belong to the vertical.
    /// </summary>
    public ImmutableArray<AltinnProject> Projects
        => _projects;

    internal AltinnVertical(
        string relPath,
        DirectoryInfo directory,
        AltinnVerticalId id,
        SemVersion version,
        ImmutableArray<AltinnProject> projects,
        AltinnVerticalConfiguration config)
    {
        _relPath = relPath;
        _directory = directory;
        _id = id;
        _version = version;
        _projects = projects;
        _config = config;

        _displayName = config.DisplayName ?? id.Name[(id.Name.LastIndexOf('.') + 1)..];
        _solutionFile = new FileInfo(Path.Combine(directory.FullName, $"{id.Name}.slnx"));

        foreach (var project in projects)
        {
            project.Vertical = this;
        }
    }

    /// <summary>
    /// Gets the direct dependencies of the vertical.
    /// </summary>
    public AltinnVerticalSet DirectDependencies
    {
        get => _directDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _directDependencies = value;
    }

    /// <summary>
    /// Gets the direct development dependencies of the vertical.
    /// </summary>
    public AltinnVerticalSet DirectDevDependencies
    {
        get => _directDevDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _directDevDependencies = value;
    }

    /// <summary>
    /// Gets all dependencies of the vertical, including transitive dependencies.
    /// </summary>
    public AltinnVerticalSet AllDependencies
    {
        get => _allDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _allDependencies = value;
    }

    /// <summary>
    /// Gets all dependencies required to build the vertical, including transitive dependencies and direct dev dependencies.
    /// </summary>
    public AltinnVerticalSet AllBuildDependencies
    {
        get => _allBuildDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _allBuildDependencies = value;
    }

    /// <summary>
    /// Gets the dependents of the vertical, i.e., the verticals that depend on this vertical.
    /// </summary>
    public AltinnVerticalSet Dependents
    {
        get => _dependents ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _dependents = value;
    }

    /// <summary>
    /// Gets a value indicating whether the vertical has been resolved, i.e., whether its dependencies and dependents have been determined.
    /// </summary>
    internal bool IsResolved => _allDependencies is not null;
}
