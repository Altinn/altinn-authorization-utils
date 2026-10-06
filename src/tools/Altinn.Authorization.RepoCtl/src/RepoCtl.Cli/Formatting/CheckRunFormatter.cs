using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.CommandLine.Formatting.Pretty;
using Altinn.Authorization.RepoCtl.Checks;
using Altinn.Authorization.RepoCtl.Model.Checks;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.RepoCtl.Formatting;

internal sealed partial class CheckRunFormatter
    : IFormatter<RichFormat>
    , IFormatter<JsonFormat>
{
    private static readonly Text SuccessIcon = new Text("✔️", Color.Green);
    private static readonly Text FailureIcon = new Text("❌", Color.Red);

    public bool CanFormat(Type type)
        => type == typeof(CheckRunResult);

    public ValueTask Format(object value, RichFormat format, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        CheckRunResult result = value switch
        {
            CheckRunResult runResult => runResult,
            _ => ThrowHelper.ThrowArgumentException<CheckRunResult>(nameof(value), "Invalid value type.")
        };

        var rows = new List<IRenderable>();
        foreach (var checkResult in result.CheckResults)
        {
            var icon = checkResult.IsSuccess ? SuccessIcon : FailureIcon;
            var tree = new Tree(new Columns(icon, new Text(checkResult.Name, Color.Yellow)) { Expand = false });

            foreach (var diag in checkResult.Diagnostics)
            {
                tree.AddNode(FormatDiagnostic(diag, result.RepositoryRoot));
            }

            rows.Add(tree);
        }

        return format.Write(new Rows(rows), writer, cancellationToken);
    }

    public ValueTask Format(object value, JsonFormat format, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        using var doc = value switch
        {
            CheckRunResult result => JsonSerializer.SerializeToDocument(result, ChecksJsonContext.Default.CheckRunResult),
            _ => ThrowHelper.ThrowArgumentException<JsonDocument>(nameof(value), "Invalid value type.")
        };

        return format.Write(doc, writer, cancellationToken);
    }

    [JsonSerializable(typeof(CheckRunResult))]
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UseStringEnumConverter = true)]
    private sealed partial class ChecksJsonContext
        : JsonSerializerContext
    {
    }

    private static IRenderable FormatDiagnostic(Diagnostic diag, DirectoryInfo repositoryRoot)
    {
        var (severityColor, severityText) = diag.Severity switch
        {
            DiagnosticSeverity.Warning => (Color.DarkOrange, " WRN "),
            DiagnosticSeverity.Error => (Color.Red, " ERR "),
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<(Color, string)>("severity", "Invalid diagnostic severity."),
        };

        var space = Notation.Text(" ");
        var origin = diag.Origin is null ? Notation.Empty : (FormatOrigin(diag.Origin, repositoryRoot) + space);
        var category = diag.Category is null ? Notation.Empty : (Notation.Text(diag.Category, Color.Magenta) + space);
        var code = diag.Code is null ? Notation.Empty : (Notation.Text(diag.Code, severityColor) + space);
        var severity = Notation.Text(severityText, new Style(background: severityColor, foreground: Color.White)) + space;
        var text = Notation.Text(diag.Text);

        return (severity + origin + category + code + text)
            | (severity + origin + Notation.Newline + code + category + text)
            | (severity + origin + Notation.Newline + code + category + Notation.Newline + text);

        static Notation FormatOrigin(DiagnosticOrigin origin, DirectoryInfo repositoryRoot)
        {
            var path = Notation.Text(MaybeToRelative(origin.Name, repositoryRoot), Color.Cyan);
            if (origin.Start.HasValue)
            {
                path += Notation.Text(":");
                path += Notation.Text(origin.Start.Line.ToString(CultureInfo.InvariantCulture), Color.Yellow);

                // TODO: column?

                if (origin.End.HasValue)
                {
                    path += Notation.Text("-");
                    path += Notation.Text(origin.End.Line.ToString(CultureInfo.InvariantCulture), Color.Yellow);
                }
            }

            return Notation.Text("[") + path + Notation.Text("]");
        }

        static string MaybeToRelative(string path, DirectoryInfo repositoryRoot)
        {
            var relative = Path.GetRelativePath(repositoryRoot.FullName, path);
            if (relative.StartsWith(".."))
            {
                return path;
            }

            return relative;
        }
    }
}
