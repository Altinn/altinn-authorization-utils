using CommunityToolkit.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Hosts an MSBuild evaluation and target-execution session.
/// </summary>
internal sealed partial class MsBuildContext
    : IDisposable
{
    private static readonly SemaphoreSlim _sharedSemaphore
        = new(1, 1);

    private readonly Lock _lock = new();
    private readonly ProjectCollection _projectCollection;
    private readonly BuildManager _buildManager;
    private readonly ILogger<MsBuildContext> _logger;
    private ushort _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsBuildContext"/> class.
    /// </summary>
    /// <param name="globalProperties">The global properties applied to every project loaded by the context.</param>
    /// <param name="loggers">The MSBuild loggers attached to the project collection.</param>
    /// <param name="logger">The logger used for build-session diagnostics.</param>
    public MsBuildContext(
        IDictionary<string, string> globalProperties,
        IEnumerable<Microsoft.Build.Framework.ILogger> loggers,
        ILogger<MsBuildContext> logger)
    {
        if (!_sharedSemaphore.Wait(0))
        {
            ThrowHelper.ThrowInvalidOperationException("Only a single instance of MsBuildContext can exist at a time.");
        }

        _logger = logger;
        _projectCollection = new ProjectCollection(
            globalProperties: globalProperties,
            loggers: loggers,
            remoteLoggers: [],
            toolsetDefinitionLocations: ToolsetDefinitionLocations.Default,
            maxNodeCount:
                Environment.GetEnvironmentVariable("REPOCTL_SINGLE_NODE", EnvironmentVariableTarget.Process)
                is "true" or "1"
                ? 1 : Environment.ProcessorCount,
            onlyLogCriticalEvents: false,
            loadProjectsReadOnly: true,
            useAsynchronousLogging: true,
            reuseProjectRootElementCache: false,
            enableTargetOutputLogging: true)
        {
            IsBuildEnabled = false,
        };

        _buildManager = BuildManager.DefaultBuildManager;
        _buildManager.BeginBuild(new BuildParameters(_projectCollection));
    }

    /// <summary>
    /// Loads and evaluates an MSBuild project.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file.</param>
    /// <returns>The evaluated project.</returns>
    public IMsBuildProject LoadProject(string projectFilePath)
    {
        EnsureNotDisposed();

        Project project;
        lock (_lock)
        {
            project = _projectCollection.LoadProject(projectFilePath);
        }

        return new MsBuildProject(this, project);
    }

    internal async Task<IMsBuildProjectSnapshot> Build(Project project, string targetName)
    {
        EnsureNotDisposed();

        BuildSubmission submission;
        lock (_lock)
        {
            var instance = project.CreateProjectInstance(ProjectInstanceSettings.None);
            var request = new BuildRequestData(instance, [targetName], hostServices: null, flags: BuildRequestDataFlags.ProvideProjectStateAfterBuild);
            submission = _buildManager.PendBuildRequest(request);
        }

        Log.BuildSubmitted(_logger, submission.SubmissionId);
        var tcs = new TaskCompletionSource<BuildResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            submission.ExecuteAsync(completed =>
            {
                try
                {
                    Log.BuildCompleted(_logger, completed.SubmissionId, completed.BuildResult?.OverallResult);

                    var buildResult = completed.BuildResult;
                    if (buildResult is null)
                    {
                        ThrowHelper.ThrowInvalidOperationException("Build result is null.");
                    }

                    if (buildResult.OverallResult != BuildResultCode.Success)
                    {
                        throw new MsBuildFailedException(buildResult);
                    }

                    tcs.SetResult(buildResult);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, null);
        }

        var result = await tcs.Task;
        if (result.ProjectStateAfterBuild is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Project state after build is null.");
        }

        return new MsBuildProjectSnapshot(result.ProjectStateAfterBuild);
    }

    private void EnsureNotDisposed()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(MsBuildContext));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // only dispose once
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
        {
            return;
        }

        // we do not dispose of the build manager, as it's a shared instance
        _buildManager.EndBuild();

        _projectCollection.UnloadAllProjects();
        _projectCollection.Dispose();

        _sharedSemaphore.Release();
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Trace, "Build submitted: {SubmissionId}")]
        public static partial void BuildSubmitted(ILogger logger, int submissionId);

        [LoggerMessage(2, LogLevel.Trace, "Build completed: {SubmissionId} with result {Result}")]
        public static partial void BuildCompleted(ILogger logger, int submissionId, BuildResultCode? result);
    }
}
