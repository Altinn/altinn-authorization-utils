using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Model;

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
                var relPath = RelPath(repository, dep.Directory);
                fullFilters.Add($"{relPath}/**");
            }

            filters.Add($"{idString}:full", fullFilters);
        }

        filters["shared"] = [
            ".github/**",
            "eng/**",
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
