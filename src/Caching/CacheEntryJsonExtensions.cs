#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Caching;

/// <summary>
/// System.Text.Json serialization extensions for <see cref="CacheEntry{T}"/>
/// </summary>
public static class CacheEntryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a cache entry to JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="value">The cache entry to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON representation of the cache entry.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson<T>(this CacheEntry<T> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a cache entry from JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized cache entry, or null if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static CacheEntry<T>? FromJson<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<CacheEntry<T>>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a cache entry from JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized cache entry, or null if deserialization failed.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson<T>(string json, out CacheEntry<T>? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<CacheEntry<T>>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}