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
    public static void AddIfNotExists<T>(this ICollection<T> collection, T item)
    {
        if (collection is not null && !collection.Contains(item))
            collection.Add(item);
    }

    /// <summary>
    /// Adds multiple items to the collection
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        if (collection is not null && items is not null)
        {
            foreach (var item in items)
                collection.Add(item);
        }
    }

    /// <summary>
    /// Removes all items matching a condition
    /// </summary>
    public static int RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        if (collection is null)
            return 0;

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
    public static IEnumerable<List<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
    {
        if (source is null || chunkSize <= 0)
            yield break;

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
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
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
    public static (List<T> True, List<T> False) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
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
    public static T? GetAtIndexOrDefault<T>(this IList<T> list, int index)
    {
        if (list is null || index < 0 || index >= list.Count)
            return default;

        return list[index];
    }

    /// <summary>
    /// Returns true if collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection is null || !collection.Any();
    }

    /// <summary>
    /// Returns true if collection has items
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() == true;
    }

    /// <summary>
    /// Converts collection to comma-separated string
    /// </summary>
    public static string ToCommaSeparatedString<T>(this IEnumerable<T> source, string separator = ", ")
    {
        if (source is null)
            return string.Empty;

        return string.Join(separator, source.Select(x => x?.ToString() ?? ""));
    }

    /// <summary>
    /// Gets random item from collection
    /// </summary>
    public static T? GetRandom<T>(this IList<T> source)
    {
        if (source is null || source.Count == 0)
            return default;

        var random = new Random();
        return source[random.Next(source.Count)];
    }

    /// <summary>
    /// Shuffles collection in place using Fisher-Yates algorithm
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list is null || list.Count <= 1)
            return;

        var random = new Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    /// <summary>
    /// Groups collection and returns dictionary with counts
    /// </summary>
    public static Dictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, int>();

        foreach (var item in source)
        {
            var key = keySelector(item);
            if (result.ContainsKey(key))
                result[key]++;
            else
                result[key] = 1;
        }

        return result;
    }

    /// <summary>
    /// Recursively flattens nested collections
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>?> childSelector)
    {
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
