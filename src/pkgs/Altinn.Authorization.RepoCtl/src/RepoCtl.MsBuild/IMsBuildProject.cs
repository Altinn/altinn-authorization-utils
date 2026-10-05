namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Represents an evaluated MSBuild project that can execute targets.
/// </summary>
public interface IMsBuildProject
    : IMsBuildProjectSnapshot
{
    /// <summary>
    /// Executes the specified target and returns the resulting project state.
    /// </summary>
    /// <param name="targetName">The name of the target to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that yields the project state after the target has completed.</returns>
    public Task<IMsBuildProjectSnapshot> Build(string targetName, CancellationToken cancellationToken = default);
}
