using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;

[DebuggerStepThrough]
[ShouldlyMethods]
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ShouldEnumerableExtensions
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static T ShouldContainSingle<T>(
        this IEnumerable<T> actual,
        Expression<Func<T, bool>> elementPredicate,
        string? customMessage = null)
    {
        var condition = elementPredicate.Compile();
        var enumerator = actual.Where(condition).GetEnumerator();

        if (!enumerator.MoveNext())
        {
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(elementPredicate.Body, actual, customMessage).ToString());
        }

        var item = enumerator.Current;

        if (enumerator.MoveNext())
        {
            throw new ShouldAssertException(new ExpectedActualShouldlyMessage(elementPredicate.Body, actual, customMessage).ToString());
        }

        return item;
    }
}
