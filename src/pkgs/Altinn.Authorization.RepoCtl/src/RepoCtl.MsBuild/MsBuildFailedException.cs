using Microsoft.Build.Execution;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Represents an exception that is thrown when an MSBuild operation fails.
/// </summary>
public sealed class MsBuildFailedException
    : Exception
{
    internal MsBuildFailedException(BuildResult buildResult)
        : base($"MSBuild failed with result: {buildResult.OverallResult}", buildResult.Exception)
    {
        BuildResult = buildResult;
    }

    /// <summary>
    /// Gets the MSBuild build result associated with this exception.
    /// </summary>
    internal BuildResult BuildResult { get; }
}
