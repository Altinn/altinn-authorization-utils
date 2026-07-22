namespace Altinn.Authorization.CommandLine.Utils;

internal static class LinqExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<T> Intersperse(T separator)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            yield return enumerator.Current;

            while (enumerator.MoveNext())
            {
                yield return separator;
                yield return enumerator.Current;
            }
        }
    }

    extension<T>(IEnumerable<T?> source)
        where T : class
    {
        public IEnumerable<T> NotNull()
            => source.Where(static x => x is not null)!;
    }
}
