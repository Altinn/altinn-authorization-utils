using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// Configuration for release-please's manifest releaser.
/// </summary>
public sealed partial record ReleasePleaseConfig
    : ReleasePleaseReleaserConfig
{
    /// <summary>
    /// Deserializes a release-please configuration from JSON.
    /// </summary>
    /// <param name="json">The JSON document to deserialize.</param>
    /// <returns>The deserialized configuration, or <see langword="null"/> when <paramref name="json"/> contains JSON <see langword="null"/>.</returns>
    public static ReleasePleaseConfig? Deserialize(string json)
        => JsonSerializer.Deserialize(json, ReleasePleaseConfigJsonSerializerContext.Default.ReleasePleaseConfig);

    /// <summary>
    /// Deserializes a release-please configuration from JSON.
    /// </summary>
    /// <param name="json">The JSON document to deserialize.</param>
    /// <returns>The deserialized configuration, or <see langword="null"/> when <paramref name="json"/> contains JSON <see langword="null"/>.</returns>
    public static ReleasePleaseConfig? Deserialize(ReadOnlySequence<byte> json)
    {
        var reader = new Utf8JsonReader(json);
        return JsonSerializer.Deserialize(ref reader, ReleasePleaseConfigJsonSerializerContext.Default.ReleasePleaseConfig);
    }

    /// <summary>
    /// Asynchronously deserializes a release-please configuration from a UTF-8 JSON stream.
    /// </summary>
    /// <param name="json">The UTF-8 JSON stream to deserialize.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The deserialized configuration, or <see langword="null"/> when <paramref name="json"/> contains JSON <see langword="null"/>.</returns>
    public static ValueTask<ReleasePleaseConfig?> DeserializeAsync(Stream json, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync(json, ReleasePleaseConfigJsonSerializerContext.Default.ReleasePleaseConfig, cancellationToken);

    /// <summary>
    /// Serializes a release-please configuration to a UTF-8 JSON stream.
    /// </summary>
    /// <param name="writer">The buffer writer to which the JSON will be written.</param>
    /// <param name="config">The release-please configuration to serialize.</param>
    public static void Serialize(IBufferWriter<byte> writer, ReleasePleaseConfig config)
    {
        var serializerOptions = ReleasePleaseConfigJsonSerializerContext.Default.Options;
        using var jsonWriter = new Utf8JsonWriter(writer, new()
        {
            Encoder = serializerOptions.Encoder,
            Indented = serializerOptions.WriteIndented,
            IndentCharacter = serializerOptions.IndentCharacter,
            IndentSize = serializerOptions.IndentSize,
            NewLine = serializerOptions.NewLine,
        });

        JsonSerializer.Serialize(jsonWriter, config, ReleasePleaseConfigJsonSerializerContext.Default.ReleasePleaseConfig);
    }

    /// <summary>
    /// Asynchronously serializes a release-please configuration to a UTF-8 JSON stream.
    /// </summary>
    /// <param name="stream">The UTF-8 JSON stream to which the configuration will be written.</param>
    /// <param name="config">The release-please configuration to serialize.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    public static Task SerializeAsync(Stream stream, ReleasePleaseConfig config, CancellationToken cancellationToken = default)
        => JsonSerializer.SerializeAsync(stream, config, ReleasePleaseConfigJsonSerializerContext.Default.ReleasePleaseConfig, cancellationToken);

    /// <summary>
    /// Gets the URI of the JSON schema used to validate the configuration.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Schema)]
    [JsonPropertyName("$schema")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Schema { get; set; }

    /// <summary>
    /// Gets the release configuration keyed by each package's path relative to the repository root.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Packages)]
    [JsonPropertyName("packages")]
    public required SortedDictionary<string, ReleasePleasePackage> Packages { get; set; }

    /// <summary>
    /// Gets the commit SHA from which release-please starts looking for commits when bootstrapping a manifest.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.BootstrapSha)]
    [JsonPropertyName("bootstrap-sha")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? BootstrapSha { get; set; }

    /// <summary>
    /// Gets the earliest commit SHA release-please may inspect when searching for previous releases.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.LastReleaseSha)]
    [JsonPropertyName("last-release-sha")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LastReleaseSha { get; set; }

    /// <summary>
    /// Gets whether Node workspace plugins always link local dependencies, even when their version ranges are satisfied.
    /// The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.AlwaysLinkLocal)]
    [JsonPropertyName("always-link-local")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AlwaysLinkLocal { get; set; }

    /// <summary>
    /// Gets the plugins to run. Entries may be plugin names or plugin configuration objects.
    /// </summary>
    /// <remarks>
    /// Built-in plugin names include <c>cargo-workspace</c>, <c>group-priority</c>, <c>linked-versions</c>,
    /// <c>maven-workspace</c>, and <c>node-workspace</c>. Configuration objects are retained as raw JSON because
    /// third-party plugins may define additional properties.
    /// </remarks>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Plugins)]
    [JsonPropertyName("plugins")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<JsonElement>? Plugins { get; set; }

    /// <summary>
    /// Gets the sign-off message appended to the release pull request's commit message.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.Signoff)]
    [JsonPropertyName("signoff")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Signoff { get; set; }

    /// <summary>
    /// Gets the template used for the title of a grouped release pull request.
    /// </summary>
    /// <remarks>The template supports the <c>${branch}</c> placeholder.</remarks>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.GroupPullRequestTitlePattern)]
    [JsonPropertyName("group-pull-request-title-pattern")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GroupPullRequestTitlePattern { get; set; }

    /// <summary>
    /// Gets the maximum number of recent releases inspected when searching for the latest release. The default is 400.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.ReleaseSearchDepth)]
    [JsonPropertyName("release-search-depth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ReleaseSearchDepth { get; set; }

    /// <summary>
    /// Gets the maximum number of commits inspected when searching for commits since the latest release. The default is 500.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.CommitSearchDepth)]
    [JsonPropertyName("commit-search-depth")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CommitSearchDepth { get; set; }

    /// <summary>
    /// Gets the number of commits fetched per GitHub API request. The default is 10.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.CommitBatchSize)]
    [JsonPropertyName("commit-batch-size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CommitBatchSize { get; set; }

    /// <summary>
    /// Gets whether GitHub API calls are made sequentially instead of concurrently. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.SequentialCalls)]
    [JsonPropertyName("sequential-calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? SequentialCalls { get; set; }

    /// <summary>
    /// Gets whether a release pull request is updated even when it contains no releasable changes. The default is <see langword="false"/>.
    /// </summary>
    [JsonPropertyOrder((int)ReleasePleaseConfigPropertyOrder.AlwaysUpdate)]
    [JsonPropertyName("always-update")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AlwaysUpdate { get; set; }

    [JsonSerializable(typeof(ReleasePleaseConfig))]
    [JsonSerializable(typeof(ReleasePleaseGenericExtraFile))]
    [JsonSerializable(typeof(ReleasePleaseJsonExtraFile))]
    [JsonSerializable(typeof(ReleasePleaseXmlExtraFile))]
    [JsonSerializable(typeof(ReleasePleaseYamlExtraFile))]
    [JsonSerializable(typeof(ReleasePleaseTomlExtraFile))]
    [JsonSerializable(typeof(ReleasePleasePomExtraFile))]
    [JsonSourceGenerationOptions(
        JsonSerializerDefaults.Web,
        PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower,
        UseStringEnumConverter = true,
        WriteIndented = true,
        IndentSize = 2,
        IndentCharacter = ' ',
        NewLine = "\n")]
    private partial class ReleasePleaseConfigJsonSerializerContext
        : JsonSerializerContext;
}
