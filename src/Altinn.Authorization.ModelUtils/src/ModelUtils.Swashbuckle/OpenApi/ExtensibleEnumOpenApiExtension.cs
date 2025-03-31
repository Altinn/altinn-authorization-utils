using Microsoft.OpenApi;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Writers;
using System.Collections;

namespace Altinn.Authorization.ModelUtils.Swashbuckle.OpenApi;

internal sealed class ExtensibleEnumOpenApiExtension
    : IList<ExtensibleEnumValue>
    , IOpenApiExtension
{
    private readonly List<ExtensibleEnumValue> _values = new();

    public ExtensibleEnumValue this[int index] 
    {
        get => _values[index];
        set => _values[index] = value;
    }

    public int Count 
        => _values.Count;

    bool ICollection<ExtensibleEnumValue>.IsReadOnly
        => ((ICollection<ExtensibleEnumValue>)_values).IsReadOnly;

    public void Add(ExtensibleEnumValue item) 
        => _values.Add(item);

    public void Clear() 
        => _values.Clear();

    public bool Contains(ExtensibleEnumValue item) 
        => _values.Contains(item);

    public void CopyTo(ExtensibleEnumValue[] array, int arrayIndex) 
        => _values.CopyTo(array, arrayIndex);

    public IEnumerator<ExtensibleEnumValue> GetEnumerator() 
        => ((IEnumerable<ExtensibleEnumValue>)_values).GetEnumerator();

    public int IndexOf(ExtensibleEnumValue item) 
        => _values.IndexOf(item);

    public void Insert(int index, ExtensibleEnumValue item) 
        => _values.Insert(index, item);

    public bool Remove(ExtensibleEnumValue item) 
        => _values.Remove(item);

    public void RemoveAt(int index) 
        => _values.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() 
        => ((IEnumerable)_values).GetEnumerator();

    void IOpenApiExtension.Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteStartArray();

        foreach (var value in _values)
        {
            ((IOpenApiExtension)value).Write(writer, specVersion);
        }

        writer.WriteEndArray();
    }
}
