#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Extension methods for collection manipulation
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Adds an item to the collection if it doesn't already exist
    /// </summary>
    /// <param name="collection">The collection to add to</param>
    /// <param name="item">The item to add</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null</exception>
    public static void AddIfNotExists<T>(this ICollection<T> collection, T item)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (!collection.Contains(item))
            collection.Add(item);
    }

    /// <summary>
    /// Adds multiple items to the collection
    /// </summary>
    /// <param name="collection">The collection to add to</param>
    /// <param name="items">The items to add</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> or <paramref name="items"/> is null</exception>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Removes all items matching a condition
    /// </summary>
    /// <param name="collection">The collection to remove from</param>
    /// <param name="predicate">The predicate to match items to remove</param>
    /// <returns>The number of items removed</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> or <paramref name="predicate"/> is null</exception>
    public static int RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);

        var itemsToRemove = collection.Where(predicate).ToList();
        int removedCount = 0;

        foreach (var item in itemsToRemove)
        {
            if (collection.Remove(item))
                removedCount++;
        }

        return removedCount;
    }

    /// <summary>
    /// Splits collection into chunks of specified size
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="chunkSize">The size of each chunk (must be positive)</param>
    /// <returns>An enumerable of chunks</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="chunkSize"/> is less than 1</exception>
    public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(chunkSize, 0);

        var chunk = new List<T>();

        foreach (var item in source)
        {
            chunk.Add(item);
            if (chunk.Count >= chunkSize)
            {
                yield return new List<T>(chunk);
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
            yield return chunk;
    }

    /// <summary>
    /// Returns distinct items by specified key selector
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="keySelector">The key selector function</param>
    /// <returns>Distinct items by key</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="keySelector"/> is null</exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var seenKeys = new HashSet<TKey>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Partitions collection into two lists based on a condition
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="predicate">The partitioning condition</param>
    /// <returns>A tuple containing (trueList, falseList)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="predicate"/> is null</exception>
    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
                trueList.Add(item);
            else
                falseList.Add(item);
        }

        return (trueList, falseList);
    }

    /// <summary>
    /// Safely gets item at index, returns default if out of bounds
    /// </summary>
    /// <param name="list">The list to access</param>
    /// <param name="index">The index to retrieve</param>
    /// <returns>The item at index or default if out of bounds</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null</exception>
    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index)
    {
        ArgumentNullException.ThrowIfNull(list);

        return index >= 0 && index < list.Count ? list[index] : default;
    }

    /// <summary>
    /// Returns true if collection is null or empty
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if null or empty</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    /// <summary>
    /// Returns true if collection has items
    /// </summary>
    /// <param name="collection">The collection to check</param>
    /// <returns>True if collection has items</returns>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() == true;
    }

    /// <summary>
    /// Converts collection to comma-separated string
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="separator">The separator to use (default: ", ")</param>
    /// <returns>Comma-separated string representation</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null</exception>
    public static string ToCommaSeparatedString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        ArgumentNullException.ThrowIfNull(source);

        return string.Join(separator, source.Select(x => x?.ToString() ?? ""));
    }

    /// <summary>
    /// Gets random item from collection
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <returns>Random item or default if collection is empty</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null</exception>
    public static T? GetRandom<T>(this IList<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Count == 0 ? default : source[Random.Shared.Next(source.Count)];
    }

    /// <summary>
    /// Shuffles collection in place using Fisher-Yates algorithm
    /// </summary>
    /// <param name="list">The list to shuffle</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null</exception>
    public static void Shuffle<T>(this IList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);

        if (list.Count <= 1)
            return;

        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    /// <summary>
    /// Groups collection and returns dictionary with counts
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="keySelector">The key selector function</param>
    /// <returns>Dictionary mapping keys to their counts</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="keySelector"/> is null</exception>
    public static Dictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        var result = new Dictionary<TKey, int>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            result[key] = result.GetValueOrDefault(key) + 1;
        }

        return result;
    }

    /// <summary>
    /// Recursively flattens nested collections
    /// </summary>
    /// <param name="source">The source collection</param>
    /// <param name="childSelector">Function to extract child collections</param>
    /// <returns>Flattened enumerable</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="childSelector"/> is null</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>?> childSelector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(childSelector);

        foreach (var item in source)
        {
            yield return item;

            var children = childSelector(item);
            if (children is not null)
            {
                foreach (var child in children.Flatten(childSelector))
                    yield return child;
            }
        }
    }
}