using Altinn.Swashbuckle.Security;
using Altinn.Swashbuckle.Tests.Serializers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(SecurityInfoSerializer), supportedTypesForSerialization: [typeof(SecurityInfo), typeof(SecurityRequirement), typeof(SecurityRequirementCondition)])]

namespace Altinn.Swashbuckle.Tests.Serializers;

public class SecurityInfoSerializer
    : IXunitSerializer
{
    public object Deserialize(Type type, string serializedValue)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(serializedValue));
        if (type == typeof(SecurityInfo))
        {
            return DeserializeSecurityInfo(ref reader);
        }
        else if (type == typeof(SecurityRequirement))
        {
            return DeserializeSecurityRequirement(ref reader);
        }
        else if (type == typeof(SecurityRequirementCondition))
        {
            return DeserializeSecurityRequirementCondition(ref reader);
        }
        else
        {
            throw new ArgumentException($"Type '{type.FullName}' is not supported by {nameof(SecurityInfoSerializer)}.");
        }
    }

    private static SecurityInfo DeserializeSecurityInfo(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray) 
        {
            throw new JsonException("Expected start of array");
        }

        var requirements = new List<SecurityRequirement>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            requirements.Add(DeserializeSecurityRequirement(ref reader));
        }

        return SecurityInfo.Create(requirements);
    }

    private static SecurityRequirement DeserializeSecurityRequirement(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected display string");
        }

        var display = reader.GetString()!;
        var conditions = new List<SecurityRequirementCondition>();

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of condition array");
        }

        while (reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            conditions.Add(DeserializeSecurityRequirementCondition(ref reader));
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException($"Expected end of array");
        }

        return SecurityRequirement.Create(display, conditions);
    }

    private static SecurityRequirementCondition DeserializeSecurityRequirementCondition(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        if (!reader.Read() || reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected scheme string");
        }

        string scheme = reader.GetString()!;
        string? scope = null;

        if (!reader.Read() || reader.TokenType is (not JsonTokenType.String) and (not JsonTokenType.EndArray))
        {
            throw new JsonException("Expected scope string or end of array");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            scope = reader.GetString();
            if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
            {
                throw new JsonException("Expected end of array");
            }
        }

        return SecurityRequirementCondition.Create(scheme, scope);
    }

    /// <inheritdoc/>
    public bool IsSerializable(Type type, object? value, [NotNullWhen(false)] out string? failureReason)
    {
        if (type == typeof(SecurityInfo) ||
            type == typeof(SecurityRequirement) ||
            type == typeof(SecurityRequirementCondition))
        {
            failureReason = null;
            return true;
        }

        failureReason = $"Type '{type.FullName}' is not supported by {nameof(SecurityInfoSerializer)}.";
        return false;
    }

    public string Serialize(object value)
    {
        var writer = new ArrayBufferWriter<byte>();

        {
            using var serializer = new Utf8JsonWriter(writer);

            switch (value)
            {
                case SecurityInfo info:
                    Serialize(info, serializer);
                    break;

                case SecurityRequirement requirement:
                    Serialize(requirement, serializer);
                    break;

                case SecurityRequirementCondition condition:
                    Serialize(condition, serializer);
                    break;

                default:
                    throw new NotSupportedException($"Type '{value.GetType().FullName}' is not supported by {nameof(SecurityInfoSerializer)}.");
            }
        }

        return Encoding.UTF8.GetString(writer.WrittenSpan);
    }

    private static void Serialize(
        SecurityInfo info,
        Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        foreach (var requirement in info)
        {
            Serialize(requirement, writer);
        }
        writer.WriteEndArray();
    }

    private static void Serialize(
        SecurityRequirement requirement,
        Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(requirement.Display);
        writer.WriteStartArray();
        foreach (var condition in requirement)
        {
            Serialize(condition, writer);
        }
        writer.WriteEndArray();
        writer.WriteEndArray();
    }

    private static void Serialize(
        SecurityRequirementCondition condition,
        Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(condition.SchemeName);
        if (condition.Scope is not null)
        {
            writer.WriteStringValue(condition.Scope);
        }
        writer.WriteEndArray();
    }
}
