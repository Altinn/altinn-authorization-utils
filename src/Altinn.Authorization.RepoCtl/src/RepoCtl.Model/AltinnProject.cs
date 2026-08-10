using System.Collections.Immutable;
using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;
using Semver;

namespace Altinn.Authorization.RepoCtl.Model;

public sealed class AltinnProject
{
    private readonly FileInfo _projectFile;
    private readonly AltinnProjectType _type;
    private readonly string _name;
    private readonly SemVersion _version;
    private AltinnVertical? _vertical;

    internal AltinnProject(FileInfo projectFile, AltinnProjectType type, string name, SemVersion version)
    {
        _projectFile = projectFile;
        _type = type;
        _name = name;
        _version = version;
    }

    public AltinnProjectType Type => _type;

    public string Name => _name;

    public FileInfo ProjectFile => _projectFile;

    public SemVersion Version => _version;

    public AltinnVertical Vertical
    {
        get => _vertical ?? ThrowHelper.ThrowInvalidOperationException<AltinnVertical>("Cannot access vertical before the project has been associated with a vertical.");
        internal set => _vertical = value;
    }
}

public enum AltinnProjectType
{
    [JsonStringEnumMemberName("exe")]
    Executable,

    [JsonStringEnumMemberName("tool")]
    Tool,

    [JsonStringEnumMemberName("lib")]
    InternalLibrary,

    [JsonStringEnumMemberName("pkg")]
    PackageLibrary,

    [JsonStringEnumMemberName("testlib")]
    TestLibrary,

    [JsonStringEnumMemberName("tests")]
    Test,

    [JsonStringEnumMemberName("sample-exe")]
    SampleExecutable,

    [JsonStringEnumMemberName("sample-lib")]
    SampleLibrary,
}

public static class AltinnProjectTypeExtensions
{
    extension(AltinnProjectType type)
    {
        public bool IsSampleProject => type is AltinnProjectType.SampleExecutable or AltinnProjectType.SampleLibrary;
        public bool IsTestProject => type is AltinnProjectType.Test or AltinnProjectType.TestLibrary;
        public bool IsTestLib => type is AltinnProjectType.TestLibrary;
        public bool CanBeReferencedByOtherProjects => type is AltinnProjectType.InternalLibrary or AltinnProjectType.PackageLibrary;
    }
}

public sealed class AltinnVertical
{
    private readonly DirectoryInfo _directory;
    private readonly FileInfo _solutionFile;
    private readonly AltinnVerticalId _id;
    private readonly SemVersion _version;
    private readonly AltinnVerticalConfiguration _config;
    private readonly ImmutableArray<AltinnProject> _projects;
    private readonly string _displayName;
    private AltinnVerticalSet? _directDependencies;
    private AltinnVerticalSet? _allDependencies;

    public DirectoryInfo Directory => _directory;
    public FileInfo SolutionFile => _solutionFile;
    public AltinnVerticalId Id => _id;

    public SemVersion Version => _version;

    public AltinnVerticalKind Kind => _id.Kind;

    public string Name => _id.Name;

    public string DisplayName => _displayName;

    public AltinnVerticalConfiguration Config => _config;

    public ImmutableArray<AltinnProject> Projects => _projects;

    internal AltinnVertical(
        DirectoryInfo directory,
        AltinnVerticalId id,
        SemVersion version,
        ImmutableArray<AltinnProject> projects,
        AltinnVerticalConfiguration config)
    {
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

    public AltinnVerticalSet DirectDependencies
    {
        get => _directDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _directDependencies = value;
    }

    public AltinnVerticalSet AllDependencies
    {
        get => _allDependencies ?? ThrowHelper.ThrowInvalidOperationException<AltinnVerticalSet>("Dependency resolution has not run yet");
        internal set => _allDependencies = value;
    }

    internal bool IsResolved => _allDependencies is not null;
}

public sealed class AltinnRepository
{
    private readonly DirectoryInfo _rootDirectory;
    private readonly FileInfo _solutionFile;
    private readonly AltinnRepositoryConfiguration _config;
    private readonly AltinnVerticalSet _verticals;
    // private readonly Solution _solution;
    // private readonly TConfig _configuration;

    public AltinnVerticalSet Verticals
        => _verticals;

    public DirectoryInfo RootDirectory
        => _rootDirectory;

    public FileInfo SolutionFile
        => _solutionFile;

    public string Name
        => _config.Name;

    internal AltinnRepository(
        DirectoryInfo rootDirectory,
        AltinnRepositoryConfiguration config,
        AltinnVerticalSet verticals)
    {
        _rootDirectory = rootDirectory;
        _solutionFile = new FileInfo(Path.Combine(rootDirectory.FullName, $"{config.Name}.slnx"));
        _config = config;
        _verticals = verticals;
    }
}

// public record AltinnVerticalConfiguration
// {
//     [JsonPropertyName("deps")]
//     internal ImmutableArray<AltinnVerticalId> Dependencies { get; init; } = [];
// }
