using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.RepoCtl.Checks;
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

            if (!checkResult.IsSuccess)
            {
                foreach (var issue in checkResult.Issues)
                {
                    tree.AddNode(issue);
                }
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
}
