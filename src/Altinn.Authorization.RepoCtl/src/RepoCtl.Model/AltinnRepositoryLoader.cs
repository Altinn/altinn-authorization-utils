using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// A loader for <see cref="AltinnRepository"/>.
/// </summary>
public sealed partial class AltinnRepositoryLoader
{
    private readonly ILogger<AltinnRepositoryLoader> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRepositoryLoader"/> class.
    /// </summary>
    public AltinnRepositoryLoader(ILogger<AltinnRepositoryLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Loads an <see cref="AltinnRepository{TConfig}"/> from the specified directory.
    /// </summary>
    /// <param name="directory">The directory containing the Altinn repository.</param>
    /// <param name="name">The optional name of the repository.</param>
    /// <returns>The loaded <see cref="AltinnRepository{TConfig}"/>.</returns>
    public async Task<AltinnRepository<TConfig>> Load<TConfig>(DirectoryInfo directory, string? name = null)
        where TConfig : AltinnRepositoryConfiguration
    {
        FileInfo solutionFile;

        if (name is null)
        {
            solutionFile = FindSolution(directory);
        }
        else
        {
            solutionFile = new FileInfo(Path.Combine(directory.FullName, $"{name}.slnx"));
        }

        Log.SolutionFile(_logger, solutionFile.FullName);
        var verticalsDir = directory.CreateSubdirectory("src"); // create if not exists

        foreach (var verticalKindDir in verticalsDir.EnumerateDirectories())
        {

        }


        throw new NotImplementedException();
    }

    private FileInfo FindSolution(DirectoryInfo directory)
    {
        var enumerator = directory.EnumerateFiles("*.slnx", SearchOption.TopDirectoryOnly).GetEnumerator();
        if (!enumerator.MoveNext())
        {
            // We require a solution file if no name is specified, as we can't create one if we don't know the name
            ThrowHelper.ThrowFileNotFoundException($"No solution file found in directory '{directory.FullName}'.");
        }

        var file = enumerator.Current;
        if (enumerator.MoveNext())
        {
            ThrowHelper.ThrowInvalidOperationException($"Multiple solution files found in directory '{directory.FullName}'.");
        }

        return file;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Debug, "Loading solution file '{SolutionFile}'")]
        public static partial void SolutionFile(ILogger logger, string solutionFile);
    }
}
