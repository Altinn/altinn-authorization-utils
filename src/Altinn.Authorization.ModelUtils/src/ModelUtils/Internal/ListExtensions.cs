namespace Altinn.Authorization.ModelUtils.Internal;

internal static class ListExtensions
{
    /// <summary>
    /// Removes the element at the specified index by swapping the last element with the element to remove and then removing the last element.
    /// </summary>
    /// <remarks>
    /// This modifies the order of the elements in the list.
    /// </remarks>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list to remove an item from.</param>
    /// <param name="index">The index of the item to remove.</param>
    public static void SwapRemoveAt<T>(this IList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, list.Count);

        list[index] = list[^1];
        list.RemoveAt(list.Count - 1);
    }
}
