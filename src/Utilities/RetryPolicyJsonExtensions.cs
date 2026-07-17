#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for RetryPolicy
/// </summary>
public static class RetryPolicyJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a RetryPolicy instance to JSON string
    /// </summary>
    /// <param name="value">The RetryPolicy instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the RetryPolicy</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this RetryPolicy value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a RetryPolicy instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A RetryPolicy instance, or null if the JSON represents a null value</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static RetryPolicy? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<RetryPolicy>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a RetryPolicy instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized RetryPolicy instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson(string json, out RetryPolicy? value)
        => TryFromJson(json.AsSpan(), out value);

    /// <summary>
    /// Attempts to deserialize a JSON string to a RetryPolicy instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized RetryPolicy instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when json is null</exception>
    public static bool TryFromJson(ReadOnlySpan<char> json, out RetryPolicy? value)
    {
        if (json.IsEmpty || json.IsWhiteSpace())
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<RetryPolicy>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
