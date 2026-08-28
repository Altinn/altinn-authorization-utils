using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.RepoCtl.Model.ReleasePlease;

/// <summary>
/// An additional file whose version references release-please updates.
/// </summary>
[JsonConverter(typeof(ReleasePleaseExtraFile.JsonConverter))]
public abstract record ReleasePleaseExtraFile
{
    private protected ReleasePleaseExtraFile(string type)
    {
        Type = type;
    }

    /// <summary>Gets the updater type.</summary>
    [JsonPropertyOrder(0)]
    [JsonPropertyName("type")]
    public string Type { get; }

    /// <summary>Gets the file path or glob relative to the package.</summary>
    [JsonPropertyOrder(1)]
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    /// <summary>Gets whether <see cref="Path"/> is interpreted as a glob. The default is <see langword="false"/>.</summary>
    [JsonPropertyOrder(2)]
    [JsonPropertyName("glob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Glob { get; set; }

    internal sealed class JsonConverter
        : JsonConverter<ReleasePleaseExtraFile>
    {
        public override ReleasePleaseExtraFile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var path = reader.GetString();

                // We already checked for a null token.
                Debug.Assert(path is not null);
                return new ReleasePleaseGenericExtraFile { Path = path };
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing {nameof(ReleasePleaseExtraFile)}.");
            }

            using var document = JsonDocument.ParseValue(ref reader);
            if (!document.RootElement.TryGetProperty("type", out var typeProperty))
            {
                throw new JsonException($"Missing required property 'type' when parsing {nameof(ReleasePleaseExtraFile)}.");
            }

            if (typeProperty.ValueKind != JsonValueKind.String)
            {
                throw new JsonException($"Unexpected type {typeProperty.ValueKind} for property 'type' when parsing {nameof(ReleasePleaseExtraFile)}.");
            }

            var type = typeProperty.GetString();
            Debug.Assert(type is not null);

            return type switch
            {
                "generic" => JsonSerializer.Deserialize<ReleasePleaseGenericExtraFile>(document, options),
                "json" => JsonSerializer.Deserialize<ReleasePleaseJsonExtraFile>(document, options),
                "xml" => JsonSerializer.Deserialize<ReleasePleaseXmlExtraFile>(document, options),
                "yaml" => JsonSerializer.Deserialize<ReleasePleaseYamlExtraFile>(document, options),
                "toml" => JsonSerializer.Deserialize<ReleasePleaseTomlExtraFile>(document, options),
                "pom" => JsonSerializer.Deserialize<ReleasePleasePomExtraFile>(document, options),
                _ => throw new JsonException($"Invalid type '{type}' when parsing {nameof(ReleasePleaseExtraFile)}."),
            };
        }

        public override void Write(Utf8JsonWriter writer, ReleasePleaseExtraFile value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

/// <summary>
/// Updates version markers in an arbitrary text file.
/// </summary>
/// <remarks>
/// The file must contain release-please's <c>x-release-please-version</c> or
/// <c>x-release-please-start-version</c>/<c>x-release-please-end</c> annotations.
/// </remarks>
public sealed record ReleasePleaseGenericExtraFile()
    : ReleasePleaseExtraFile("generic");

/// <summary>
/// Updates a value selected by JSONPath in a JSON file.
/// </summary>
public sealed record ReleasePleaseJsonExtraFile()
    : ReleasePleaseExtraFile("json")
{
    /// <summary>Gets the JSONPath selecting the version value to update.</summary>
    [JsonPropertyOrder(3)]
    [JsonPropertyName("jsonpath")]
    public required string JsonPath { get; set; }
}

/// <summary>
/// Updates a value selected by XPath in an XML file.
/// </summary>
public sealed record ReleasePleaseXmlExtraFile()
    : ReleasePleaseExtraFile("xml")
{
    /// <summary>Gets the XPath selecting the version value to update.</summary>
    [JsonPropertyOrder(3)]
    [JsonPropertyName("xpath")]
    public required string XPath { get; set; }
}

/// <summary>
/// Updates a value selected by JSONPath in a YAML file.
/// </summary>
public sealed record ReleasePleaseYamlExtraFile()
    : ReleasePleaseExtraFile("yaml")
{
    /// <summary>Gets the JSONPath selecting the version value to update.</summary>
    [JsonPropertyOrder(3)]
    [JsonPropertyName("jsonpath")]
    public required string JsonPath { get; set; }
}

/// <summary>
/// Updates a value selected by JSONPath in a TOML file.
/// </summary>
public sealed record ReleasePleaseTomlExtraFile()
    : ReleasePleaseExtraFile("toml")
{
    /// <summary>Gets the JSONPath selecting the version value to update.</summary>
    [JsonPropertyOrder(3)]
    [JsonPropertyName("jsonpath")]
    public required string JsonPath { get; set; }
}

/// <summary>
/// Updates the version in a Maven <c>pom.xml</c> file.
/// </summary>
public sealed record ReleasePleasePomExtraFile()
    : ReleasePleaseExtraFile("pom");
