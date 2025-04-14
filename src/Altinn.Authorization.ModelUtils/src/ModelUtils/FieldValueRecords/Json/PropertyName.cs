using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Json;

[DebuggerDisplay("{_name}")]
internal class PropertyName
{
    private readonly string _name;
    private readonly JsonEncodedText _encoded;

    internal static string ConvertName(string name, JsonNamingPolicy? propertyNamingPolicy)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        if (propertyNamingPolicy is not null)
        {
            name = propertyNamingPolicy.ConvertName(name);
        }

        return name;
    }

    internal static PropertyName Create(string name, JsonNamingPolicy? propertyNamingPolicy)
    {
        name = ConvertName(name, propertyNamingPolicy);

        var encoded = JsonEncodedText.Encode(name);
        return new PropertyName(name, encoded);
    }

    private PropertyName(string name, JsonEncodedText encoded)
    {
        _name = name;
        _encoded = encoded;
    }

    public string Name => _name;

    public JsonEncodedText Encoded => _encoded;

    public override string ToString()
        => _name;

    internal class Comparer
        : IEqualityComparer<PropertyName>
        , IAlternateEqualityComparer<string, PropertyName?>
        , IAlternateEqualityComparer<ReadOnlySpan<char>, PropertyName?>
    {
        public static readonly Comparer Ordinal = new(StringComparer.Ordinal);
        public static readonly Comparer OrdinalIgnoreCase = new(StringComparer.OrdinalIgnoreCase);

        private readonly IEqualityComparer<string> _inner;
        private readonly IAlternateEqualityComparer<ReadOnlySpan<char>, string?> _innerSpan;

        private Comparer(IEqualityComparer<string> inner)
        {
            Guard.IsAssignableToType<IAlternateEqualityComparer<ReadOnlySpan<char>, string?>>(inner);

            _inner = inner;
            _innerSpan = (IAlternateEqualityComparer<ReadOnlySpan<char>, string?>)_inner;
        }

        PropertyName? IAlternateEqualityComparer<string, PropertyName?>.Create(string alternate)
        {
            throw new NotSupportedException("It's not possible to create PropertyNames using the PropertyNameComparer");
        }

        PropertyName? IAlternateEqualityComparer<ReadOnlySpan<char>, PropertyName?>.Create(ReadOnlySpan<char> alternate)
        {
            throw new NotSupportedException("It's not possible to create PropertyNames using the PropertyNameComparer");
        }

        public bool Equals(PropertyName? x, PropertyName? y)
            => _inner.Equals(x?.Name, y?.Name);

        public bool Equals(string alternate, PropertyName? other)
            => _inner.Equals(alternate, other?.Name);

        public bool Equals(ReadOnlySpan<char> alternate, PropertyName? other)
            => _innerSpan.Equals(alternate, other?.Name);

        public int GetHashCode([DisallowNull] PropertyName obj)
            => _inner.GetHashCode(obj.Name);

        public int GetHashCode(string alternate)
            => _inner.GetHashCode(alternate);

        public int GetHashCode(ReadOnlySpan<char> alternate)
            => _innerSpan.GetHashCode(alternate);
    }

}
