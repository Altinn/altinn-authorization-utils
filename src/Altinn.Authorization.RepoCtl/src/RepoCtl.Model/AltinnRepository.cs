namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents an Altinn repository, which is a collection of verticals and their projects.
/// </summary>
public sealed class AltinnRepository
{
    private readonly DirectoryInfo _rootDirectory;
    private readonly FileInfo _solutionFile;
    private readonly AltinnRepositoryConfiguration _config;
    private readonly AltinnVerticalSet _verticals;

    /// <summary>
    /// Gets the verticals in the repository.
    /// </summary>
    public AltinnVerticalSet Verticals
        => _verticals;

    /// <summary>
    /// Gets the root directory of the repository.
    /// </summary>
    public DirectoryInfo RootDirectory
        => _rootDirectory;

    /// <summary>
    /// Gets the solution file (.slnx) of the repository.
    /// </summary>
    public FileInfo SolutionFile
        => _solutionFile;

    /// <summary>
    /// Gets the name of the repository.
    /// </summary>
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
