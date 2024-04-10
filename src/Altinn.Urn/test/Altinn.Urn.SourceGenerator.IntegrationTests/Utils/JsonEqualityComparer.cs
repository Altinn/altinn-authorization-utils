using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Urn.SourceGenerator.IntegrationTests.Utils;

internal class JsonEqualityComparer
    : IEqualityComparer<JsonElement>
    , IEqualityComparer<JsonDocument>
    , IEquivalencyStep
{
    public static JsonEqualityComparer Instance { get; } = new JsonEqualityComparer();

    /// <inheritdoc/>
    public bool Equals(JsonDocument? x, JsonDocument? y)
    {
        if (x is null)
        {
            return y is null;
        }

        if (y is null)
        {
            return false;
        }

        return Equals(x.RootElement, y.RootElement);
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] JsonDocument obj)
    {
        if (obj is null)
        {
            return 0;
        }

        return GetHashCode(obj.RootElement);
    }

    /// <inheritdoc/>
    public bool Equals(JsonElement x, JsonElement y)
    {
        if (x.ValueKind != y.ValueKind)
        {
            return false;
        }

        return x.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null => true,
            JsonValueKind.Number => x.GetDecimal() == y.GetDecimal(),
            JsonValueKind.String => x.GetString() == y.GetString(),
            JsonValueKind.Object => CompareObjects(x, y),
            JsonValueKind.Array => CompareArrays(x, y),
            _ => ThrowArgumentOutOfRangeException<bool>(nameof(x.ValueKind)),
        };
    }

    /// <inheritdoc/>
    public int GetHashCode([DisallowNull] JsonElement obj)
    {
        return obj.ValueKind switch
        {
            JsonValueKind.Undefined or JsonValueKind.True or JsonValueKind.False or JsonValueKind.Null => obj.ValueKind.GetHashCode(),
            JsonValueKind.String => obj.GetString()!.GetHashCode(StringComparison.Ordinal),
            JsonValueKind.Number => obj.GetDecimal().GetHashCode(),
            JsonValueKind.Object => GetObjectHashCode(obj),
            JsonValueKind.Array => GetArrayHashCode(obj),
            _ => ThrowArgumentOutOfRangeException<int>(nameof(obj.ValueKind)),
        };
    }

    public EquivalencyResult Handle(
        Comparands comparands, 
        IEquivalencyValidationContext context,
        IEquivalencyValidator nestedValidator)
    {
        if (comparands.GetExpectedType(context.Options) == typeof(JsonDocument))
        {
            return HandleJsonDocument(comparands, context, nestedValidator);
        }

        if (comparands.GetExpectedType(context.Options) == typeof(JsonElement))
        {
            return HandleJsonElement(comparands, context, nestedValidator);
        }

        return EquivalencyResult.ContinueWithNext;
    }

    private EquivalencyResult HandleJsonDocument(
        Comparands comparands,
        IEquivalencyValidationContext context,
        IEquivalencyValidator nestedValidator)
    {
        Execute.Assertion
            .BecauseOf(context.Reason.FormattedMessage, context.Reason.Arguments)
            .ForCondition(comparands.Subject is JsonDocument)
            .FailWith("Expected {context:object} to be of type {0}{because}, but found {1}", typeof(JsonDocument), comparands.Subject)
            .Then
            .Given(() => Equals((JsonDocument)comparands.Subject, (JsonDocument)comparands.Expectation))
            .ForCondition(isEqual => isEqual)
            .FailWith("Expected {context:object} to be equal to {1} according to {0}{because}, but {2} was not.", nameof(JsonEqualityComparer), comparands.Expectation, comparands.Subject);

        return EquivalencyResult.AssertionCompleted;
    }

    private EquivalencyResult HandleJsonElement(
        Comparands comparands,
        IEquivalencyValidationContext context,
        IEquivalencyValidator nestedValidator)
    {
        Execute.Assertion
            .BecauseOf(context.Reason.FormattedMessage, context.Reason.Arguments)
            .ForCondition(comparands.Subject is JsonElement)
            .FailWith("Expected {context:object} to be of type {0}{because}, but found {1}", typeof(JsonElement), comparands.Subject)
            .Then
            .Given(() => Equals((JsonElement)comparands.Subject, (JsonElement)comparands.Expectation))
            .ForCondition(isEqual => isEqual)
            .FailWith("Expected {context:object} to be equal to {1} according to {0}{because}, but {2} was not.", nameof(JsonEqualityComparer), comparands.Expectation, comparands.Subject);

        return EquivalencyResult.AssertionCompleted;
    }

    public override string ToString()
    {
        return $"Use {nameof(JsonEqualityComparer)} for json objects";
    }

    private bool CompareArrays(JsonElement x, JsonElement y)
    {
        if (x.GetArrayLength() != y.GetArrayLength())
        {
            return false;
        }

        var selfEnumerator = x.EnumerateArray();
        var otherEnumerator = y.EnumerateArray();

        while (selfEnumerator.MoveNext() && otherEnumerator.MoveNext())
        {
            if (!Equals(selfEnumerator.Current, otherEnumerator.Current))
            {
                return false;
            }
        }

        return true;
    }

    private bool CompareObjects(JsonElement x, JsonElement y)
    {
        var propNames = ArrayPool<string>.Shared.Rent(8);
        var index = 0;
        try
        {
            foreach (var prop in x.EnumerateObject())
            {
                if (index > propNames.Length - 1)
                {
                    // Resize the array
                    var newPropNames = ArrayPool<string>.Shared.Rent(propNames.Length * 2);

                    try
                    {
                        propNames.AsSpan().CopyTo(newPropNames.AsSpan(0, propNames.Length));
                        (newPropNames, propNames) = (propNames, newPropNames);
                    }
                    finally
                    {
                        ArrayPool<string>.Shared.Return(newPropNames);
                    }
                }

                propNames[index++] = prop.Name;
            }

            var span = propNames.AsSpan(0, index);

            foreach (var prop in y.EnumerateObject())
            {
                if (!span.Contains(prop.Name))
                {
                    return false;
                }
            }

            foreach (var propName in span)
            {
                var selfProp = x.GetProperty(propName);
                if (!y.TryGetProperty(propName, out var otherProp))
                {
                    return false;
                }

                if (!Equals(selfProp, otherProp))
                {
                    return false;
                }
            }
        }
        finally
        {
            ArrayPool<string>.Shared.Return(propNames);
        }

        return true;
    }

    private int GetArrayHashCode(JsonElement obj)
    {
        var hash = default(HashCode);
        foreach (var item in obj.EnumerateArray())
        {
            hash.Add(item, this);
        }

        return hash.ToHashCode();
    }

    private int GetObjectHashCode(JsonElement obj)
    {
        var hash = default(HashCode);
        var propNames = ArrayPool<string>.Shared.Rent(8);
        var index = 0;
        try
        {
            foreach (var prop in obj.EnumerateObject())
            {
                if (index > propNames.Length - 1)
                {
                    // Resize the array
                    var newPropNames = ArrayPool<string>.Shared.Rent(propNames.Length * 2);

                    try
                    {
                        propNames.AsSpan().CopyTo(newPropNames.AsSpan(0, propNames.Length));
                        (newPropNames, propNames) = (propNames, newPropNames);
                    }
                    finally
                    {
                        ArrayPool<string>.Shared.Return(newPropNames);
                    }
                }

                propNames[index++] = prop.Name;
            }

            var span = propNames.AsSpan(0, index);

            // Sort the keys so that we always get the same result
            span.Sort(StringComparer.Ordinal);

            foreach (var propName in span)
            {
                var prop = obj.GetProperty(propName);
                hash.Add(propName, StringComparer.Ordinal);
                hash.Add(prop, this);
            }
        }
        finally
        {
            ArrayPool<string>.Shared.Return(propNames);
        }

        return hash.ToHashCode();
    }

    static T ThrowArgumentOutOfRangeException<T>(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName);
    }
}
