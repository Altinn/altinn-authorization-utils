using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.Json;

internal interface IUrnJsonWrapper<TSelf, T>
    where TSelf : IUrnJsonWrapper<TSelf, T>
    where T : IKeyValueUrn<T>
{
    public T? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    [return: NotNullIfNotNull(nameof(value))]
    public static abstract implicit operator TSelf?(T? value);
}
