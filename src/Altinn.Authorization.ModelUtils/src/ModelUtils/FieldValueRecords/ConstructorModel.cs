using System.Collections.Immutable;
using System.Reflection;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords;

/// <summary>
/// A model for a field-value-record constructor.
/// </summary>
/// <typeparam name="T">The field-value-record type.</typeparam>
internal class ConstructorModel<T>
    : IFieldValueRecordConstructorModel<T>
    where T : class
{
    private readonly ConstructorInvoker _invoker;

    public ConstructorModel(
        ConstructorInfo ctor,
        ImmutableArray<IFieldValueRecordConstructorParameterModel> parameters)
    {
        _invoker = ConstructorInvoker.Create(ctor);
        Parameters = parameters;
    }

    public ImmutableArray<IFieldValueRecordConstructorParameterModel> Parameters { get; }

    public T Invoke(Span<object?> parameters)
        => (T)_invoker.Invoke(parameters);
}
