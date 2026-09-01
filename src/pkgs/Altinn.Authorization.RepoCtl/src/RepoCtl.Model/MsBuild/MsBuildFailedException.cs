using Microsoft.Build.Execution;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

internal sealed class MsBuildFailedException
    : Exception
{
    public MsBuildFailedException(BuildResult buildResult)
        : base($"MSBuild failed with result: {buildResult.OverallResult}", buildResult.Exception)
    {
        BuildResult = buildResult;
    }

    public BuildResult BuildResult { get; }
}
