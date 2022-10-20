#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Validation helpers for collection operations that work with CollectionExtensions
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates a collection for issues that would cause problems with CollectionExtensions methods
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="collectionName">Optional name of the collection for error messages</param>
    /// <returns>List of human-readable validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null</exception>
    public static IReadOnlyList<string> Validate<T>(
        this IEnumerable<T>? collection,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(collection))] string? collectionName = null)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = new List<string>();
        var name = collectionName ?? "collection";

        // Validate for methods that expect non-null items
        var nullItems = collection.Where(x => x is null).ToList();
        if (nullItems.Count > 0)
        {
            problems.Add(string.Format("{0} contains {1} null item(s). CollectionExtensions methods may throw NullReferenceException.", name, nullItems.Count));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a list for index-based operations
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to validate</param>
    /// <param name="index">The index to check</param>
    /// <param name="indexName">Optional name of the index parameter for error messages</param>
    /// <returns>List of human-readable validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null</exception>
    public static IReadOnlyList<string> ValidateIndex<T>(
        this IList<T>? list,
        int index,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(index))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(list);

        var problems = new List<string>();
        var name = indexName ?? "index";

        if (index < 0)
        {
            problems.Add(string.Format("{0} ({1}) cannot be negative.", name, index));
        }

        if (index >= list.Count)
        {
            problems.Add(string.Format("{0} ({1}) is out of range for list with {2} item(s). Maximum valid index is {3}.", name, index, list.Count, list.Count - 1));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates chunk size for Chunk method
    /// </summary>
    /// <param name="chunkSize">The chunk size to validate</param>
    /// <param name="chunkSizeName">Optional name of the chunkSize parameter for error messages</param>
    /// <returns>List of human-readable validation problems, empty if valid</returns>
    public static IReadOnlyList<string> ValidateChunkSize(
        int chunkSize,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(chunkSize))] string? chunkSizeName = null)
    {
        var problems = new List<string>();
        var name = chunkSizeName ?? "chunkSize";

        if (chunkSize <= 0)
        {
            problems.Add(string.Format("{0} ({1}) must be a positive integer. Chunk size must be 1 or greater.", name, chunkSize));
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a collection is valid for CollectionExtensions operations
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to check</param>
    /// <param name="collectionName">Optional name of the collection for error messages</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid<T>(
        this IEnumerable<T>? collection,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(collection))] string? collectionName = null)
    {
        return collection.Validate(collectionName).Count == 0;
    }

    /// <summary>
    /// Checks if an index is valid for list operations
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to check</param>
    /// <param name="index">The index to validate</param>
    /// <param name="indexName">Optional name of the index parameter for error messages</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidIndex<T>(
        this IList<T>? list,
        int index,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(index))] string? indexName = null)
    {
        return list.ValidateIndex(index, indexName).Count == 0;
    }

    /// <summary>
    /// Checks if a chunk size is valid
    /// </summary>
    /// <param name="chunkSize">The chunk size to check</param>
    /// <param name="chunkSizeName">Optional name of the chunkSize parameter for error messages</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidChunkSize(
        int chunkSize,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(chunkSize))] string? chunkSizeName = null)
    {
        return ValidateChunkSize(chunkSize, chunkSizeName).Count == 0;
    }

    /// <summary>
    /// Ensures a collection is valid for CollectionExtensions operations, throwing if not
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="collectionName">Optional name of the collection for error messages</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValid<T>(
        this IEnumerable<T>? collection,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(collection))] string? collectionName = null)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var problems = collection.Validate(collectionName);

        if (problems.Count > 0)
        {
            var message = string.Format("Collection validation failed for {0}:\n{1}",
                collectionName ?? "collection",
                string.Join("\n", problems));
            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Ensures an index is valid for list operations, throwing if not
    /// </summary>
    /// <typeparam name="T">The type of elements in the list</typeparam>
    /// <param name="list">The list to validate</param>
    /// <param name="index">The index to check</param>
    /// <param name="indexName">Optional name of the index parameter for error messages</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with a list of problems</exception>
    public static void EnsureValidIndex<T>(
        this IList<T>? list,
        int index,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(index))] string? indexName = null)
    {
        ArgumentNullException.ThrowIfNull(list);

        var problems = list.ValidateIndex(index, indexName);

        if (problems.Count > 0)
        {
            var message = string.Format("Index validation failed for {0} ({1}):\n{2}",
                indexName ?? "index",
                index,
                string.Join("\n", problems));
            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Ensures a chunk size is valid, throwing if not
    /// </summary>
    /// <param name="chunkSize">The chunk size to validate</param>
    /// <param name="chunkSizeName">Optional name of the chunkSize parameter for error messages</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public static void EnsureValidChunkSize(
        int chunkSize,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(chunkSize))] string? chunkSizeName = null)
    {
        var problems = ValidateChunkSize(chunkSize, chunkSizeName);

        if (problems.Count > 0)
        {
            var message = string.Format("Chunk size validation failed for {0} ({1}):\n{2}",
                chunkSizeName ?? "chunkSize",
                chunkSize,
                string.Join("\n", problems));
            throw new ArgumentException(message);
        }
    }
}
