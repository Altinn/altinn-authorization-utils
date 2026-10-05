using System.Collections;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using CommunityToolkit.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.RepoCtl.Model.MsBuild;

/// <summary>
/// Hosts an MSBuild evaluation session.
/// </summary>
public interface IMsBuildContext
    : IDisposable
{
    /// <summary>
    /// Loads and evaluates an MSBuild project.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file.</param>
    /// <returns>The evaluated project.</returns>
    public IMsBuildProject LoadProject(string projectFilePath);
}

internal sealed partial class MsBuildContext
    : IMsBuildContext
{
    private readonly Lock _lock = new();
    private readonly ProjectCollection _projectCollection;
    private ushort _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsBuildContext"/> class.
    /// </summary>
    /// <param name="globalProperties">The global properties applied to every project loaded by the context.</param>
    /// <param name="logger">The logger used for build-session diagnostics.</param>
    public MsBuildContext(
        IDictionary<string, string> globalProperties,
        ILogger<MsBuildContext> logger)
    {
        _projectCollection = new ProjectCollection(
            globalProperties: globalProperties,
            loggers: [new MsBuildLoggerAdapter(logger)],
            remoteLoggers: [],
            toolsetDefinitionLocations: ToolsetDefinitionLocations.Default,
            maxNodeCount: 0,
            onlyLogCriticalEvents: false,
            loadProjectsReadOnly: true,
            useAsynchronousLogging: true,
            reuseProjectRootElementCache: false,
            enableTargetOutputLogging: true)
        {
            IsBuildEnabled = false,
        };
    }

    /// <inheritdoc/>
    public IMsBuildProject LoadProject(string projectFilePath)
    {
        EnsureNotDisposed();

        Project project;
        lock (_lock)
        {
            project = _projectCollection.LoadProject(projectFilePath);
        }

        return new MsBuildProject(project);
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

        _projectCollection.UnloadAllProjects();
        _projectCollection.Dispose();
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
}
