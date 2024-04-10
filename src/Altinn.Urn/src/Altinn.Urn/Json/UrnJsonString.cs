using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Json;

[JsonConverter(typeof(StringUrnJsonConverter))]
public readonly struct UrnJsonString<T>
    : IUrnJsonWrapper<UrnJsonString<T>, T>
    where T : IKeyValueUrn<T>
{
    public T? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    private UrnJsonString(T? value)
    {
        Value = value;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (ReferenceEquals(Value, obj))
        {
            return true;
        }

        if (ReferenceEquals(Value, null))
        {
            return false;
        }

        return Value.Equals(obj);
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? "";
    }

    public static implicit operator UrnJsonString<T>(T? value)
        => new(value);
}
