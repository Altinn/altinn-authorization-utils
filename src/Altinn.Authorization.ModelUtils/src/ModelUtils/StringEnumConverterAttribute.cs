using CommunityToolkit.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils;

/// <summary>
/// Uses a <see cref="JsonStringEnumConverter"/> to serialize and deserialize enums as strings.
/// </summary>
[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public class StringEnumConverterAttribute
    : JsonConverterAttribute
{
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly JsonStringEnumConverter _converter;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumConverterAttribute"/> class.
    /// </summary>
    /// <param name="namingPolicy">The <see cref="JsonNamingPolicy"/> to use for enum cases.</param>
    protected StringEnumConverterAttribute(JsonNamingPolicy? namingPolicy)
    {
        _namingPolicy = namingPolicy;
        _converter = new(namingPolicy, allowIntegerValues: false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumConverterAttribute"/> class.
    /// </summary>
    /// <param name="namingPolicy">The <see cref="JsonKnownNamingPolicy"/> to use for enum cases.</param>
    public StringEnumConverterAttribute(JsonKnownNamingPolicy namingPolicy)
        : this(GetNamingPolicy(namingPolicy))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringEnumConverterAttribute"/> class.
    /// </summary>
    public StringEnumConverterAttribute()
        : this(null)
    {
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert)
        => _converter;

    /// <summary>
    /// Gets the <see cref="JsonStringEnumConverter"/>.
    /// </summary>
    public JsonStringEnumConverter Converter
        => _converter;

    /// <summary>
    /// Gets the <see cref="JsonNamingPolicy"/> to use for enum cases.
    /// </summary>
    public JsonNamingPolicy? NamingPolicy
        => _namingPolicy;

    static JsonNamingPolicy? GetNamingPolicy(JsonKnownNamingPolicy namingPolicy)
        => namingPolicy switch
        {
            JsonKnownNamingPolicy.Unspecified => null,
            JsonKnownNamingPolicy.CamelCase => JsonNamingPolicy.CamelCase,
            JsonKnownNamingPolicy.KebabCaseLower => JsonNamingPolicy.KebabCaseLower,
            JsonKnownNamingPolicy.KebabCaseUpper => JsonNamingPolicy.KebabCaseUpper,
            JsonKnownNamingPolicy.SnakeCaseLower => JsonNamingPolicy.SnakeCaseLower,
            JsonKnownNamingPolicy.SnakeCaseUpper => JsonNamingPolicy.SnakeCaseUpper,
            _ => ThrowHelper.ThrowNotSupportedException<JsonNamingPolicy>($"Unsupported {nameof(JsonKnownNamingPolicy)} value: {namingPolicy}"),
        };
}
