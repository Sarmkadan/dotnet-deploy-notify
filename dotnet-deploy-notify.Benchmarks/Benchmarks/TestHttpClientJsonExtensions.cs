using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// System.Text.Json helpers for <see cref="TestHttpClient"/>.
/// </summary>
public static class TestHttpClientJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Converts a <see cref="TestHttpClient"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="TestHttpClient"/> to convert.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="TestHttpClient"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this TestHttpClient value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? _jsonSerializerOptions with { WriteIndented = true } : _jsonSerializerOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="TestHttpClient"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="TestHttpClient"/> or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static TestHttpClient? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            return JsonSerializer.Deserialize<TestHttpClient>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="TestHttpClient"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="TestHttpClient"/> if successful.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out TestHttpClient? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<TestHttpClient>(json, _jsonSerializerOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
