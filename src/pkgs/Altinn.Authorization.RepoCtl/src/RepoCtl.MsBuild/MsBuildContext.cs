using System.Collections;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Hosts an MSBuild evaluation and target-execution session.
/// </summary>
public interface IMsBuildContext
    : IDisposable
{
    /// <summary>
    /// Loads and evaluates an MSBuild project.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The evaluated project.</returns>
    public Task<IMsBuildProject> LoadProject(string projectFilePath, CancellationToken cancellationToken = default);
}

internal sealed partial class MsBuildContext
    : IMsBuildContext
{
    private static readonly SemaphoreSlim _sharedSemaphore
        = new(1, 1);

    private readonly Lock _lock = new();
    private readonly ProjectCollection _projectCollection;
    private readonly BuildManager? _buildManager;
    private readonly ILogger<MsBuildContext> _logger;
    private ushort _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsBuildContext"/> class.
    /// </summary>
    /// <param name="globalProperties">The global properties applied to every project loaded by the context.</param>
    /// <param name="logger">The logger used for build-session diagnostics.</param>
    /// <param name="isDesignTime">Indicates whether the context is being created for design-time purposes.</param>
    /// <remarks>Design-time contexts do not support building projects.</remarks>
    public MsBuildContext(
        IDictionary<string, string> globalProperties,
        ILogger<MsBuildContext> logger,
        bool isDesignTime)
    {
        if (!isDesignTime && !_sharedSemaphore.Wait(0))
        {
            ThrowHelper.ThrowInvalidOperationException("Only a single instance of MsBuild build-context can exist at a time.");
        }

        _logger = logger;
        _projectCollection = new ProjectCollection(
            globalProperties: globalProperties,
            loggers: [new MsBuildLoggerAdapter(logger)],
            remoteLoggers: [],
            toolsetDefinitionLocations: ToolsetDefinitionLocations.Default,
            maxNodeCount: isDesignTime ? 0
                : (Environment.GetEnvironmentVariable("REPOCTL_SINGLE_NODE", EnvironmentVariableTarget.Process)
                    is "true" or "1"
                    ? 1 : Environment.ProcessorCount),
            onlyLogCriticalEvents: false,
            loadProjectsReadOnly: true,
            useAsynchronousLogging: true,
            reuseProjectRootElementCache: false,
            enableTargetOutputLogging: true)
        {
            IsBuildEnabled = false,
        };

        if (!isDesignTime)
        {
            _buildManager = BuildManager.DefaultBuildManager;
            _buildManager.BeginBuild(new BuildParameters(_projectCollection));
        }
    }

    /// <summary>
    /// Gets a value indicating whether the context is only used for design-time purposes.
    /// </summary>
    public bool IsDesignTime
        => _buildManager is null;

    /// <inheritdoc/>
    public Task<IMsBuildProject> LoadProject(string projectFilePath, CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        Project project;
        lock (_lock)
        {
            project = _projectCollection.LoadProject(projectFilePath);
        }

        return Task.FromResult<IMsBuildProject>(new MsBuildProject(this, project));
    }

    internal async Task<IMsBuildProjectSnapshot> Build(Project project, string targetName, CancellationToken cancellationToken)
    {
        EnsureNotDisposed();

        if (_buildManager is null)
        {
            ThrowHelper.ThrowInvalidOperationException("Cannot build in a design-time context.");
        }

        BuildSubmission submission;
        lock (_lock)
        {
            var instance = project.CreateProjectInstance(ProjectInstanceSettings.None);
            var request = new BuildRequestData(instance, [targetName], hostServices: null, flags: BuildRequestDataFlags.ProvideProjectStateAfterBuild);
            submission = _buildManager.PendBuildRequest(request);
        }

        cancellationToken.ThrowIfCancellationRequested();
        Log.BuildSubmitted(_logger, submission.SubmissionId);
        var tcs = new TaskCompletionSource<BuildResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            submission.ExecuteAsync(completed =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        return;
                    }

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

        var result = await tcs.Task.WaitAsync(cancellationToken);
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
        _buildManager?.EndBuild();

        _projectCollection.UnloadAllProjects();
        _projectCollection.Dispose();

        if (!IsDesignTime)
        {
            _sharedSemaphore.Release();
        }
    }

    private sealed class MsBuildLoggerAdapter(ILogger logger)
        : Microsoft.Build.Framework.ILogger
    {
        private readonly Func<BuildEventState, Exception?, string> _format = BuildEventState.Format;

        // unused
        public Microsoft.Build.Framework.LoggerVerbosity Verbosity { get; set; }

        // unused
        public string? Parameters
        {
            get => null;
            set => throw new NotSupportedException();
        }

        public void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
        {
            eventSource.MessageRaised += (_, e) => Log(e, ToLogLevel(e.Importance));
            eventSource.WarningRaised += (_, e) => Log(e, LogLevel.Warning);
            eventSource.ErrorRaised += (_, e) => Log(e, LogLevel.Error);

            void Log(Microsoft.Build.Framework.BuildEventArgs e, LogLevel level)
            {
                if (!logger.IsEnabled(level))
                {
                    return;
                }

                var code = e switch
                {
                    Microsoft.Build.Framework.BuildMessageEventArgs message => message.Code,
                    Microsoft.Build.Framework.BuildWarningEventArgs warning => warning.Code,
                    Microsoft.Build.Framework.BuildErrorEventArgs error => error.Code,
                    _ => null,
                };

                var eventId = string.IsNullOrEmpty(code)
                    ? default
                    : new EventId(
                        unchecked((int)XxHash32.HashToUInt32(MemoryMarshal.AsBytes(code.AsSpan()))),
                        code);
                logger.Log(level, eventId, new BuildEventState(e), null, _format);
            }
        }

        public void Shutdown()
        {
        }

        private static LogLevel ToLogLevel(Microsoft.Build.Framework.MessageImportance importance)
            => importance switch
            {
                Microsoft.Build.Framework.MessageImportance.Low => LogLevel.Trace,
                Microsoft.Build.Framework.MessageImportance.Normal => LogLevel.Debug,
                Microsoft.Build.Framework.MessageImportance.High => LogLevel.Information,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<LogLevel>(nameof(importance), importance, null),
            };
    }

    private readonly struct BuildEventState(Microsoft.Build.Framework.BuildEventArgs e)
        : IReadOnlyList<KeyValuePair<string, object?>>
    {
        public KeyValuePair<string, object?> this[int index]
            => index switch
            {
                0 => new("OriginalFormat", "{Message}"),
                1 => new("Message", e.Message),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<KeyValuePair<string, object?>>(nameof(index)),
            };

        public int Count => 2;

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private string Format(Exception? exception)
        {
            if (exception is null)
            {
                return e.Message ?? string.Empty;
            }

            var msg = e.Message;
            if (string.IsNullOrEmpty(msg))
            {
                return exception.ToString();
            }

            return $"{msg}: {exception}";
        }

        public static string Format(BuildEventState state, Exception? exception)
            => state.Format(exception);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Trace, "Build submitted: {SubmissionId}")]
        public static partial void BuildSubmitted(ILogger logger, int submissionId);

        [LoggerMessage(2, LogLevel.Trace, "Build completed: {SubmissionId} with result {Result}")]
        public static partial void BuildCompleted(ILogger logger, int submissionId, BuildResultCode? result);
    }
}
