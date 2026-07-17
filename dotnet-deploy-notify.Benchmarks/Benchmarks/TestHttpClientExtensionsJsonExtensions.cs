using System.Text.Json;

namespace DotNetDeployNotify.Benchmarks.Benchmarks;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="TestHttpClientExtensions"/>
/// </summary>
public static class TestHttpClientExtensionsJsonExtensions
{
    /// <summary>
    /// JSON serialization options with camelCase naming policy
    /// </summary>
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="TestHttpClientExtensions"/> instance to a JSON string
    /// </summary>
    /// <param name="value">The instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON representation of the instance</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this TestHttpClientExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _jsonOptions with { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="TestHttpClientExtensions"/> instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>The deserialized instance, or null if JSON is null or empty</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    /// <exception cref="JsonException">JSON is invalid or cannot be deserialized</exception>
    public static TestHttpClientExtensions? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<TestHttpClientExtensions>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="TestHttpClientExtensions"/> instance
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter containing the deserialized instance</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/></exception>
    public static bool TryFromJson(string json, out TestHttpClientExtensions? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<TestHttpClientExtensions>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}