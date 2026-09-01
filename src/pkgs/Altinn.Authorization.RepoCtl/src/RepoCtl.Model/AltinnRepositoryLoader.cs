using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Altinn.Authorization.ModelUtils;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.RepoCtl.Model.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Semver;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// A loader for <see cref="AltinnRepository"/>.
/// </summary>
public interface IAltinnRepositoryLoader
{
    /// <summary>
    /// Loads an <see cref="AltinnRepository"/> by starting from the specified directory and searching upwards for a git repository root.
    /// </summary>
    /// <param name="directory">The directory containing the Altinn repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The loaded <see cref="AltinnRepository"/>.</returns>
    Task<Result<AltinnRepository>> Load(DirectoryInfo directory, CancellationToken cancellationToken = default);
}

/// <summary>
/// A loader for <see cref="AltinnRepository"/>.
/// </summary>
internal sealed partial class AltinnRepositoryLoader
    : IAltinnRepositoryLoader
{
    private readonly ILogger<AltinnRepositoryLoader> _logger;
    private readonly IEnumerable<Microsoft.Build.Framework.ILogger> _msbuildLoggers;

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRepositoryLoader"/> class.
    /// </summary>
    public AltinnRepositoryLoader(ILogger<AltinnRepositoryLoader> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _msbuildLoggers = [new MsBuildLoggerAdapter(loggerFactory.CreateLogger<Microsoft.Build.Framework.ILogger>())];
    }

    /// <inheritdoc/>
    public async Task<Result<AltinnRepository>> Load(DirectoryInfo directory, CancellationToken cancellationToken = default)
    {
        await using var findConfigResult = directory.FindUp(
            [".repo"],
            ["json", "jsonc"]);

        if (findConfigResult is null)
        {
            return Problems.RepositoryRootNotFound;
        }

        var rootDir = findConfigResult.FileInfo.DirectoryName!;
        Log.ConfigPath(_logger, findConfigResult.FileInfo.FullName);
        var configResult = await AltinnRepositoryConfiguration.Read(findConfigResult, cancellationToken);
        if (configResult.IsProblem)
        {
            return Problems.InvalidConfig.Create(
                detail: $"Failed to read repository configuration from '{findConfigResult.FileInfo.FullName}'",
                source: configResult.Problem);
        }

        var config = configResult.Value;
        Log.Config(_logger, config);

        var solutionFile = new FileInfo(Path.Combine(rootDir, $"{config.Name}.slnx"));
        var srcDir = new DirectoryInfo(Path.Combine(rootDir, "src"));
        var rootDirs = srcDir.GetDirectories().Select(dir =>
        {
            AltinnVerticalKind.TryParseDirName(dir.Name, null, out var kind);

            return new
            {
                Dir = dir,
                Kind = kind,
            };
        })
        .ToList();

        Dictionary<AltinnVerticalKind, DirectoryInfo> kindDirs;
        if (rootDirs.All(dir => !dir.Kind.HasValue))
        {
            var kind = config.RootKind;
            if (!kind.HasValue)
            {
                return Problems.RootModeWithoutKind;
            }

            Log.RootMode(_logger, kind);
            kindDirs = new Dictionary<AltinnVerticalKind, DirectoryInfo>(1)
            {
                [kind] = srcDir,
            };
        }
        else if (rootDirs.Any(dir => !dir.Kind.HasValue))
        {
            return Problems.MixedModeRepository;
        }
        else
        {
            kindDirs = new Dictionary<AltinnVerticalKind, DirectoryInfo>(rootDirs.Count);
            foreach (var dir in rootDirs)
            {
                Debug.Assert(dir.Kind.HasValue);
                kindDirs[dir.Kind] = dir.Dir; // it should not be possible for this to overwrite
            }
        }

        var verticalDirs = new List<(AltinnVerticalKind Kind, DirectoryInfo Dir)>();
        foreach (var (kind, dir) in kindDirs)
        {
            foreach (var verticalDir in dir.GetDirectories())
            {
                verticalDirs.Add((kind, verticalDir));
            }
        }

        var channel = Channel.CreateBounded<Result<AltinnVertical>>(new BoundedChannelOptions(Environment.ProcessorCount)
        {
            SingleReader = true,
            SingleWriter = false,
        });

        var reader = channel.Reader;
        var consumerTask = Task.Run<Result<AltinnVerticalSet>>(async () =>
        {
            MultipleProblemBuilder problems = default;
            var builder = ImmutableArray.CreateBuilder<AltinnVertical>();
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                if (item.IsProblem)
                {
                    problems.Add(item.Problem);
                }
                else
                {
                    var vertical = item.Value;
                    builder.Add(vertical);
                }
            }

            if (problems.TryBuild(out var problem))
            {
                return problem;
            }

            return AltinnVerticalSet.Create(builder);
        }, cancellationToken);

        var rootDirInfo = new DirectoryInfo(rootDir);
        var writer = channel.Writer;
        var producerTask = Task.Run(async () =>
        {
            try
            {
                await Parallel.ForEachAsync(verticalDirs, cancellationToken, (tpl, ct) => LoadVertical(rootDirInfo, tpl.Kind, tpl.Dir, writer, ct));
            }
            catch (Exception ex)
            {
                writer.TryComplete(ex);
                throw;
            }
            finally
            {
                writer.TryComplete();
            }
        }, cancellationToken);

        await Task.WhenAll(producerTask, consumerTask);

        Debug.Assert(consumerTask.IsCompletedSuccessfully);
        var verticalsResult = await consumerTask;
        if (verticalsResult.IsProblem)
        {
            return verticalsResult.Problem;
        }

        var verticals = verticalsResult.Value;
        if (AltinnVerticalDependencyResolver.ResolveDependencies(verticals) is { IsProblem: true, Problem: var dependencyProblem })
        {
            return dependencyProblem;
        }

        return new AltinnRepository(rootDirInfo, config, verticals);
    }

    private async ValueTask LoadVertical(DirectoryInfo rootDirInfo, AltinnVerticalKind kind, DirectoryInfo directory, ChannelWriter<Result<AltinnVertical>> writer, CancellationToken cancellationToken)
    {
        await using var findConfigResult = directory.Find(
            [".vertical", "conf"],
            ["json", "jsonc"]);

        await using var findVersionResult = directory.Find(
            ["version"],
            ["txt"]);

        AltinnVerticalConfiguration config;
        if (findConfigResult is null)
        {
            config = AltinnVerticalConfiguration.Default;
        }
        else
        {
            var configResult = await AltinnVerticalConfiguration.Read(findConfigResult, cancellationToken);
            if (configResult.IsProblem)
            {
                var configProblem = Problems.InvalidConfig.Create(
                    detail: $"Failed to read vertical configuration from '{findConfigResult.FileInfo.FullName}'",
                    source: configResult.Problem);
                await writer.WriteAsync(configProblem, cancellationToken);
                return;
            }

            config = configResult.Value;
        }

        SemVersion? version;
        if (findVersionResult is null)
        {
            version = new SemVersion(0, 0, 0);
        }
        else
        {
            var versionString = await new StreamReader(findVersionResult).ReadToEndAsync(cancellationToken);
            if (!SemVersion.TryParse(versionString.TrimEnd(), SemVersionStyles.Strict, out version))
            {
                var versionProblem = Problems.InvalidVersion.Create(
                    detail: $"Failed to parse version from '{findVersionResult.FileInfo.FullName}'",
                    extensions: [new("version", versionString)]);
                await writer.WriteAsync(versionProblem, cancellationToken);
                return;
            }
        }

        var id = new AltinnVerticalId(kind, directory.Name);

        // check if it has any projects
        var matcher = new Matcher();
        matcher.AddInclude("src/*/*.csproj");
        matcher.AddInclude("test/*/*.csproj");
        matcher.AddInclude("sample/*/*.csproj");

        var result = matcher.Execute(new DirectoryInfoWrapper(directory));
        if (!result.HasMatches)
        {
            Log.NoProjects(_logger, kind, directory.FullName);
            return;
        }

        var globalProperties = new Dictionary<string, string>
        {
            ["DesignTimeBuild"] = "true",
        };

        using var collection = new ProjectCollection(
            globalProperties: globalProperties,
            loggers: _msbuildLoggers,
            toolsetDefinitionLocations: ToolsetDefinitionLocations.Default);

        var projects = result.Files.TryGetNonEnumeratedCount(out var count)
            ? ImmutableArray.CreateBuilder<AltinnProject>(count)
            : ImmutableArray.CreateBuilder<AltinnProject>();
        foreach (var projectFileMatch in result.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dirKind = projectFileMatch.Path.AsSpan(0, 3) switch
            {
                "src" => AltinnProjectDirKind.Source,
                "tes" => AltinnProjectDirKind.Test,
                "sam" => AltinnProjectDirKind.Sample,
                _ => throw new InvalidOperationException($"Unexpected project file path '{projectFileMatch.Path}'"),
            };

            var projectFile = new FileInfo(Path.Combine(directory.FullName, projectFileMatch.Path));
            var msbuildProject = collection.LoadProject(projectFile.FullName);
            var name = msbuildProject.GetPropertyValue("MSBuildProjectName");
            var versionString = msbuildProject.GetPropertyValue("Version");

            SemVersion? projectVersion;
            if (string.IsNullOrWhiteSpace(versionString))
            {
                Log.ProjectHasNoVersion(_logger, name, msbuildProject.FullPath);
                projectVersion = new SemVersion(0, 0, 0);
            }
            else if (!SemVersion.TryParse(versionString, SemVersionStyles.Strict, out projectVersion))
            {
                Log.ProjectHasInvalidVersion(_logger, name, msbuildProject.FullPath, versionString);
                projectVersion = new SemVersion(0, 0, 0);
            }

            var type = GetProjectType(msbuildProject, dirKind, kind, name, projectVersion);

            var project = new AltinnProject(projectFile, type, name, projectVersion);
            projects.Add(project);
        }

        projects.SortBy(static project => (project.Type, project.Name));
        var vertical = new AltinnVertical(
            GetRelativePath(rootDirInfo.FullName, directory.FullName),
            directory,
            id,
            version,
            projects.DrainToImmutable(),
            config);

        await writer.WriteAsync(vertical, cancellationToken);
    }

    private AltinnProjectType GetProjectType(Project project, AltinnProjectDirKind dirKind, AltinnVerticalKind verticalKind, string name, SemVersion version)
    {
        var isTestProject = project.GetPropertyValueAsBool("IsTestProject");
        var isSampleProject = project.GetPropertyValueAsBool("IsSampleProject");
        var isTestLibrary = project.GetPropertyValueAsBool("IsTestLibrary");
        var isPackable = project.GetPropertyValueAsBool("IsPackable");
        var outputType = project.GetPropertyValue("OutputType");
        var isTool = project.GetPropertyValueAsBool("PackAsTool");
        var isExe = string.Equals(outputType, "exe", StringComparison.OrdinalIgnoreCase);

        Log.ProjectInfo(_logger, project.FullPath, name, version, isTestProject, isSampleProject, isTestLibrary, isPackable, outputType, isTool);

        AltinnProjectType type;
        if (dirKind == AltinnProjectDirKind.Source)
        {
            if (verticalKind == AltinnVerticalKind.Package)
            {
                type = AltinnProjectType.PackageLibrary;
            }
            else if (verticalKind == AltinnVerticalKind.Tool)
            {
                type = AltinnProjectType.Tool;

                if (!isExe)
                {
                    Log.ProjectShouldBeExecutable(_logger, name, project.FullPath);
                }
            }
            else
            {
                type = isExe
                    ? AltinnProjectType.Executable
                    : AltinnProjectType.InternalLibrary;
            }

            if (isExe && !CanBeExecutable(verticalKind))
            {
                Log.ProjectShouldNotBeExecutable(_logger, name, project.FullPath);
            }

            if (isPackable && !CanBePackable(verticalKind))
            {
                Log.ProjectShouldNotBePackable(_logger, name, project.FullPath);
            }

            if (isTool && !CanBeTool(verticalKind))
            {
                Log.ProjectShouldNotBeTool(_logger, name, project.FullPath);
            }

            if (isTestProject)
            {
                Log.ProjectShouldNotBeTestProject(_logger, name, project.FullPath);
            }

            if (isSampleProject)
            {
                Log.ProjectShouldNotBeSampleProject(_logger, name, project.FullPath);
            }
        }
        else if (dirKind == AltinnProjectDirKind.Test)
        {
            type = isTestProject
                ? AltinnProjectType.Test
                : AltinnProjectType.TestLibrary;

            if (!isTestProject && !isTestLibrary)
            {
                Log.ProjectShouldBeTestProjectOrLibrary(_logger, name, project.FullPath);
            }

            if (isPackable)
            {
                Log.ProjectShouldNotBePackable(_logger, name, project.FullPath);
            }

            if (isSampleProject)
            {
                Log.ProjectShouldNotBeSampleProject(_logger, name, project.FullPath);
            }

            if (isTool)
            {
                Log.ProjectShouldNotBeTool(_logger, name, project.FullPath);
            }
        }
        else if (dirKind == AltinnProjectDirKind.Sample)
        {
            type = isExe
                ? AltinnProjectType.SampleExecutable
                : AltinnProjectType.SampleLibrary;

            if (!isSampleProject)
            {
                Log.ProjectShouldBeSampleProject(_logger, name, project.FullPath);
            }

            if (isTestProject)
            {
                Log.ProjectShouldNotBeTestProject(_logger, name, project.FullPath);
            }

            if (isTestLibrary)
            {
                Log.ProjectShouldNotBeTestLibrary(_logger, name, project.FullPath);
            }

            if (isPackable)
            {
                Log.ProjectShouldNotBePackable(_logger, name, project.FullPath);
            }

            if (isTool)
            {
                Log.ProjectShouldNotBeTool(_logger, name, project.FullPath);
            }
        }
        else
        {
            return ThrowHelper.ThrowArgumentOutOfRangeException<AltinnProjectType>(nameof(dirKind), dirKind, null);
        }

        return type;

        static bool CanBeExecutable(AltinnVerticalKind kind)
            => kind == AltinnVerticalKind.Application || kind == AltinnVerticalKind.Tool;

        static bool CanBePackable(AltinnVerticalKind kind)
            => kind == AltinnVerticalKind.Package || kind == AltinnVerticalKind.Tool;

        static bool CanBeTool(AltinnVerticalKind kind)
            => kind == AltinnVerticalKind.Tool;
    }

    private static string GetRelativePath(string rootPath, string fullPath)
    {
        var relativePath = Path.GetRelativePath(rootPath, fullPath);
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private enum AltinnProjectDirKind
    {
        Source,
        Test,
        Sample,
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
            eventSource.MessageRaised += (_, e) =>
            {
                var level = ToLogLevel(e.Importance);
                if (!logger.IsEnabled(level))
                {
                    return;
                }

                var id = XxHash32.HashToUInt32(MemoryMarshal.AsBytes(e.Code.AsSpan()));
                var eventId = new EventId(unchecked((int)id), e.Code);
                logger.Log(level, eventId, new BuildEventState(e), null, _format);
            };

            // eventSource.WarningRaised
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
        [LoggerMessage(1, LogLevel.Debug, "Loading solution file '{SolutionFile}'")]
        public static partial void SolutionFile(ILogger logger, string solutionFile);

        [LoggerMessage(2, LogLevel.Debug, "Loading repository configuration from '{ConfigPath}'")]
        public static partial void ConfigPath(ILogger logger, string configPath);

        [LoggerMessage(3, LogLevel.Debug, "Loaded repository configuration: {@Config}")]
        public static partial void Config(ILogger logger, AltinnRepositoryConfiguration config);

        [LoggerMessage(4, LogLevel.Debug, "Repository is in 'root' mode with kind '{Kind}'")]
        public static partial void RootMode(ILogger logger, AltinnVerticalKind kind);

        [LoggerMessage(5, LogLevel.Debug, "No projects found in vertical '{Kind}' at '{Directory}'")]
        public static partial void NoProjects(ILogger logger, AltinnVerticalKind kind, string directory);

        [LoggerMessage(6, LogLevel.Debug, "Project '{ProjectFile}' info: Name={Name}, Version={Version}, IsTestProject={IsTestProject}, IsSampleProject={IsSampleProject}, IsTestLibrary={IsTestLibrary}, IsPackable={IsPackable}, OutputType={OutputType}, IsTool={IsTool}")]
        public static partial void ProjectInfo(ILogger logger, string projectFile, string name, SemVersion version, bool isTestProject, bool isSampleProject, bool isTestLibrary, bool isPackable, string outputType, bool isTool);

        [LoggerMessage(7, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be executable")]
        public static partial void ProjectShouldNotBeExecutable(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(8, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be packable")]
        public static partial void ProjectShouldNotBePackable(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(9, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be a tool")]
        public static partial void ProjectShouldNotBeTool(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(10, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be a test project")]
        public static partial void ProjectShouldNotBeTestProject(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(11, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be a test library")]
        public static partial void ProjectShouldNotBeTestLibrary(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(12, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should not be a sample project")]
        public static partial void ProjectShouldNotBeSampleProject(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(13, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should be a test project or test library")]
        public static partial void ProjectShouldBeTestProjectOrLibrary(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(14, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should be a sample project")]
        public static partial void ProjectShouldBeSampleProject(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(15, LogLevel.Warning, "Project '{ProjectName}' at '{ProjectFile}' should be executable")]
        public static partial void ProjectShouldBeExecutable(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(16, LogLevel.Error, "Project '{ProjectName}' at '{ProjectFile}' has no version specified")]
        public static partial void ProjectHasNoVersion(ILogger logger, string projectName, string projectFile);

        [LoggerMessage(17, LogLevel.Error, "Project '{ProjectName}' at '{ProjectFile}' has an invalid version '{VersionString}'")]
        public static partial void ProjectHasInvalidVersion(ILogger logger, string projectName, string projectFile, string versionString);
    }
}
