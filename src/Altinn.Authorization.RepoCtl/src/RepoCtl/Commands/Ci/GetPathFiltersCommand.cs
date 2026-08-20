using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Arguments;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed partial class GetPathFiltersCommand(IGitHubActionsService actions)
{
    public async Task<ICommandResult> Invoke(AltinnRepository repository, AltinnVerticalSet verticals, CancellationToken cancellationToken = default)
    {
        var filters = new SortedDictionary<string, SortedSet<string>>();
        foreach (var vertical in verticals)
        {
            var idString = vertical.Id.ToString();
            var relPath = RelPath(repository, vertical.Directory);
            filters.Add(idString, [$"{relPath}/**"]);

            if (vertical.Kind == AltinnVerticalKind.Application)
            {
                filters.Add($"{idString}:infra", [$"{relPath}/infra/**"]);
            }
        }

        foreach (var vertical in verticals)
        {
            var idString = vertical.Id.ToString();
            SortedSet<string> fullFilters = [.. filters[idString]];

            foreach (var dep in vertical.AllDependencies)
            {
                fullFilters.UnionWith(filters[dep.Id.ToString()]);
            }

            filters.Add($"{idString}:full", fullFilters);
        }

        filters["shared"] = [
            ".github/**",
            "Directory.Build.props",
            "Directory.Build.targets",
            "Directory.Packages.props",
            "Altinn.ruleset",
            "Stylecop.json",
            "src/Directory.Build.props",
            "src/Directory.Build.targets",
            "src/Directory.Packages.props",
            "src/Altinn.ruleset",
            "src/Stylecop.json",
            "src/.gitignore",
            ".editorconfig",
            ".gitignore",
            "global.json",
        ];

        var doc = JsonSerializer.SerializeToDocument(filters, PathsFilterJsonContext.Default.Filters);
        if (actions.IsGitHubActions)
        {
            await actions.SetOutput("pathsFilters", doc.RootElement.GetRawText(), cancellationToken);
        }

        return JsonResult.From(doc);
    }

    private static string RelPath(AltinnRepository repository, FileSystemInfo fileSystemEntry)
    {
        var root = repository.RootDirectory.FullName;
        var path = fileSystemEntry.FullName;

        return Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    [JsonSerializable(typeof(SortedDictionary<string, SortedSet<string>>), TypeInfoPropertyName = "Filters")]
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UseStringEnumConverter = true)]
    private sealed partial class PathsFilterJsonContext
        : JsonSerializerContext;
}

internal sealed partial class FindVerticalsCommand(IGitHubActionsService actions)
{
    public async Task<ICommandResult> Invoke(
        AltinnRepository repository,
        AltinnVerticalSet verticals,
        IConsole console,
        [Option("--filter", Description = "Filter based on changed paths.")] ChangedFilter filter = ChangedFilter.None,
        CancellationToken cancellationToken = default)
    {
        ChangedPaths changedPaths = ChangedPaths.Read();

        var infos = verticals.AsEnumerable()
            .Select(v => new VerticalInfo(repository, v, changedPaths))
            .Where(FilterChanged(filter))
            .ToImmutableArray();

        var json = JsonSerializer.SerializeToDocument(infos, FindVerticalsJsonContext.Default.Output);
        if (actions.IsGitHubActions)
        {
            var matrix = new VerticalsMatrix { Verticals = infos };
            var matrixJson = JsonSerializer.Serialize(matrix, FindVerticalsJsonContext.Default.Matrix);
            await actions.SetOutput("matrix", matrixJson, cancellationToken);
            await actions.SetOutput("verticals", json.RootElement.GetRawText(), cancellationToken);
            await actions.SetOutput("any", infos.Length > 0 ? "\"true\"" : "\"false\"", cancellationToken);
        }

        return JsonResult.From(json);
    }

    private static Func<VerticalInfo, bool> FilterChanged(ChangedFilter filter)
        => filter switch
        {
            ChangedFilter.None => _ => true,
            ChangedFilter.Infra => v => v.HasChangesInfra,
            ChangedFilter.Self => v => v.HasChanges,
            ChangedFilter.SelfOnly => v => v.HasChangesSelfOnly,
            ChangedFilter.Full => v => v.HasChangesFull,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null),
        };

    private static string RelPath(AltinnRepository repository, FileSystemInfo fileSystemEntry)
    {
        var root = repository.RootDirectory.FullName;
        var path = fileSystemEntry.FullName;

        return Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    }

    private sealed class VerticalsMatrix
    {
        [JsonPropertyName("displayName")]
        public IEnumerable<string> DisplayNames
            => Verticals.Select(v => v.DisplayName);

        [JsonPropertyName("include")]
        public ImmutableArray<VerticalInfo> Verticals { get; init; }
    }

    private sealed class VerticalInfo
    {
        private readonly AltinnRepository _repository;
        private readonly AltinnVertical _vertical;
        private readonly ChangedPaths _changedPaths;

        internal VerticalInfo(AltinnRepository repository, AltinnVertical vertical, ChangedPaths changedPaths)
        {
            _repository = repository;
            _vertical = vertical;
            _changedPaths = changedPaths;
        }

        [JsonPropertyName("id")]
        public string Id
            => _vertical.Id.ToString();

        [JsonPropertyName("slug")]
        public string Slug
            => _vertical.Id.ToString("s", null);

        [JsonPropertyName("displayName")]
        public string DisplayName
            => _vertical.DisplayId;

        [JsonPropertyName("shortName")]
        public string ShortName
            => _vertical.DisplayName;

        [JsonPropertyName("path")]
        public string Path
            => RelPath(_repository, _vertical.Directory);

        [JsonPropertyName("name")]
        public string Name
            => _vertical.FullName;

        [JsonPropertyName("type")]
        public AltinnVerticalKind Type
            => _vertical.Kind;

        [JsonPropertyName("infra")]
        public bool HasInfra
            => _vertical.Config.Infrastructure is not null;

        [JsonPropertyName("terraform")]
        public bool HasTerraform
            => _vertical.Config.Infrastructure?.Terraform is not null;

        [JsonPropertyName("changed")]
        public bool HasChanges
            => _changedPaths.HasChanges(_vertical.Id.ToString());

        [JsonPropertyName("changedOnly")]
        public bool HasChangesSelfOnly
            => _changedPaths.HasChanges(_vertical.Id.ToString(), includeShared: false);

        [JsonPropertyName("changedInfra")]
        public bool HasChangesInfra
            => _changedPaths.HasChanges($"{_vertical.Id}:infra");

        [JsonPropertyName("changedFull")]
        public bool HasChangesFull
            => _changedPaths.HasChanges($"{_vertical.Id}:full");
    }

    [ArgumentEnum]
    public enum ChangedFilter
    {
        [ArgumentValues("none")]
        None,

        [ArgumentValues("infra")]
        Infra,

        [ArgumentValues("self")]
        Self,

        [ArgumentValues("self-only", "selfOnly")]
        SelfOnly,

        [ArgumentValues("full")]
        Full,
    }

    private sealed record ChangedPaths
    {
        private static readonly ChangedPaths _empty = new([]);

        public static ChangedPaths Read()
        {
            var envValue = Environment.GetEnvironmentVariable("PATHS_CHANGED");
            if (string.IsNullOrEmpty(envValue))
            {
                return _empty;
            }

            var deserialized = JsonSerializer.Deserialize(envValue, FindVerticalsJsonContext.Default.Input);
            if (deserialized is null)
            {
                return _empty;
            }

            return new ChangedPaths(deserialized);
        }

        private readonly bool _sharedChanged;
        private readonly ImmutableArray<string> _filtersWithChanges;

        public ChangedPaths(Dictionary<string, bool> input)
        {
            var builder = ImmutableArray.CreateBuilder<string>(input.Count);
            foreach (var (filter, hasChanges) in input)
            {
                if (string.Equals(filter, "shared", StringComparison.Ordinal))
                {
                    _sharedChanged = hasChanges;
                    continue;
                }

                if (hasChanges)
                {
                    builder.Add(filter);
                }
            }

            builder.Sort(StringComparer.Ordinal);
            _filtersWithChanges = builder.ToImmutable();
        }

        public bool HasChanges(string filter, bool includeShared = true)
        {
            if (includeShared && _sharedChanged)
            {
                return true;
            }

            return _filtersWithChanges.BinarySearch(filter, StringComparer.Ordinal) >= 0;
        }
    }

    [JsonSerializable(typeof(Dictionary<string, bool>), TypeInfoPropertyName = "Input")]
    [JsonSerializable(typeof(ImmutableArray<VerticalInfo>), TypeInfoPropertyName = "Output")]
    [JsonSerializable(typeof(VerticalsMatrix), TypeInfoPropertyName = "Matrix")]
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UseStringEnumConverter = true,
        Converters = [typeof(BooleanFromStringConverter)])]
    private sealed partial class FindVerticalsJsonContext
        : JsonSerializerContext;

    private sealed class BooleanFromStringConverter
        : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is JsonTokenType.True)
            {
                return true;
            }

            if (reader.TokenType is JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType is JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            throw new JsonException($"Cannot convert {reader.TokenType} to boolean.");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
