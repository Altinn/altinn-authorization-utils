using CommunityToolkit.Diagnostics;
using Semver;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents a (C#) project in an Altinn repository.
/// </summary>
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

    /// <summary>
    /// Gets the type of the project.
    /// </summary>
    public AltinnProjectType Type
        => _type;

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public string Name
        => _name;

    /// <summary>
    /// Gets the project file (.csproj) of the project.
    /// </summary>
    public FileInfo ProjectFile
        => _projectFile;

    /// <summary>
    /// Gets the version of the project.
    /// </summary>
    public SemVersion Version
        => _version;

    /// <summary>
    /// Gets the vertical that the project belongs to.
    /// </summary>
    public AltinnVertical Vertical
    {
        get => _vertical ?? ThrowHelper.ThrowInvalidOperationException<AltinnVertical>("Cannot access vertical before the project has been associated with a vertical.");
        internal set => _vertical = value;
    }
}
