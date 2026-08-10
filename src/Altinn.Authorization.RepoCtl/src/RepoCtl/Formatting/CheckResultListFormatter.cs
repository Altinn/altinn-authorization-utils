using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authorization.CommandLine.Formatting;
using Altinn.Authorization.RepoCtl.Checks;
using CommunityToolkit.Diagnostics;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Formatting;

internal sealed partial class CheckResultListFormatter
    : IFormatter<RichFormat>
    , IFormatter<JsonFormat>
{
    public bool CanFormat(Type type)
        => type == typeof(CheckResult) || type.IsAssignableTo(typeof(IEnumerable<CheckResult>));

    public ValueTask Format(object value, RichFormat format, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        IEnumerable<CheckResult> results = value switch
        {
            CheckResult result => [result],
            IEnumerable<CheckResult> resultList => resultList,
            _ => ThrowHelper.ThrowArgumentException<IEnumerable<CheckResult>>(nameof(value), "Value must be a CheckResult or an IEnumerable<CheckResult>.")
        };

        var paragraph = new Paragraph();
        foreach (var checkResult in results)
        {
            if (checkResult.IsSuccess)
            {
                paragraph.Append("✔️", Color.Green);
            }
            else
            {
                paragraph.Append("❌", Color.Red);
            }
            paragraph.Append(" ");
            paragraph.Append(checkResult.Name, Color.Yellow);

            if (!checkResult.IsSuccess)
            {
                foreach (var issue in checkResult.Issues)
                {
                    paragraph.Append("\n");
                    paragraph.Append("  [");
                    paragraph.Append(issue.File, Color.Cyan);
                    paragraph.Append("] ");
                    paragraph.Append(issue.Message);
                }
            }

            paragraph.Append("\n");
        }

        return format.Write(paragraph, writer, cancellationToken);
    }

    public ValueTask Format(object value, JsonFormat format, IFormatWriter writer, CancellationToken cancellationToken = default)
    {
        using var doc = value switch
        {
            CheckResult result => JsonSerializer.SerializeToDocument(result, ChecksJsonContext.Default.CheckResult),
            IEnumerable<CheckResult> results => JsonSerializer.SerializeToDocument(results, ChecksJsonContext.Default.IEnumerableCheckResult),
            _ => ThrowHelper.ThrowArgumentException<JsonDocument>(nameof(value), "Value must be a CheckResult or an IEnumerable<CheckResult>.")
        };

        return format.Write(doc, writer, cancellationToken);
    }

    [JsonSerializable(typeof(IEnumerable<CheckResult>))]
    [JsonSerializable(typeof(CheckResult))]
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
