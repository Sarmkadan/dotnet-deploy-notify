using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Provides JSON serialization helpers for <see cref="CacheBenchmarks"/>.
/// </summary>
public static class CacheBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Converts the specified <see cref="CacheBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="CacheBenchmarks"/> instance to convert.</param>
    /// <param name="indented">Whether to format the JSON string with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="CacheBenchmarks"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this CacheBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CacheBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="CacheBenchmarks"/> instance if deserialization is successful; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static CacheBenchmarks? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
        }

        try
        {
            return JsonSerializer.Deserialize<CacheBenchmarks>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="CacheBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="CacheBenchmarks"/> instance if successful; otherwise, null.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out CacheBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON string cannot be empty or whitespace.", nameof(json));
        }

        try
        {
            value = JsonSerializer.Deserialize<CacheBenchmarks>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}