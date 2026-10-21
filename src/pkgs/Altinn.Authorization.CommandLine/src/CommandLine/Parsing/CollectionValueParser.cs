using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Parsing;

internal sealed class CollectionValueParser<TCollection, TElement>
{
    public bool TryParse(ValueParsingContext context, out TCollection value, [NotNullWhen(false)] out string? error)
    {
        IList<TElement> values = typeof(TCollection).IsArray
            ? new TElement[context.Values.Count]
            : new List<TElement>(context.Values.Count);

        for (var i = 0; i < context.Values.Count; i++)
        {
            if (!context.TryParse<TElement>([context.Values[i]], out var parsed, out error))
            {
                value = default!;
                return false;
            }

            if (values is TElement[] array)
            {
                array[i] = parsed;
            }
            else
            {
                values.Add(parsed);
            }
        }

        value = (TCollection)values;
        error = null;
        return true;
    }
}
