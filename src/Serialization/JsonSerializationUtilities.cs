#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNetDeployNotify.Serialization;

/// <summary>
/// Provides unified JSON serialization utilities for consistent error handling and configuration
/// across all JSON extension classes in the application.
/// </summary>
/// <remarks>
/// This class standardizes:
/// - Exception handling for JSON deserialization (wraps JsonException in domain-specific exceptions)
/// - Null and empty input validation
/// - Consistent naming policy and enum handling
/// - Common serialization options for internal data structures
/// </remarks>
public static class JsonSerializationUtilities
{
    /// <summary>
    /// Default JsonSerializerOptions for internal data serialization.
    /// Uses camelCase naming policy, allows case-insensitive property matching,
    /// and ignores null values during serialization.
    /// </summary>
    public static JsonSerializerOptions DefaultInternalOptions { get; } = new(SecureJsonSerializerOptions.InternalData)
    {
        // Ensure consistent property naming policy
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// JsonSerializerOptions for indented JSON output.
    /// Same as DefaultInternalOptions but with WriteIndented = true.
    /// </summary>
    public static JsonSerializerOptions DefaultInternalOptionsIndented { get; } = new JsonSerializerOptions(DefaultInternalOptions)
    {
        WriteIndented = true
    };

    /// <summary>
    /// Safely deserializes JSON while handling common error cases consistently.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="options">The JsonSerializerOptions to use</param>
    /// <returns>The deserialized object, or null if deserialization fails</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static T? SafeDeserialize<T>(string json, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (JsonException ex)
        {
            // Return null on deserialization failure to maintain consistent behavior
            // Callers can use TryDeserialize for more control
            return default;
        }
    }

    /// <summary>
    /// Attempts to deserialize JSON with consistent error handling.
    /// </summary>
    /// <typeparam name="T">The type to deserialize</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="options">The JsonSerializerOptions to use</param>
    /// <param name="value">Receives the deserialized value if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryDeserialize<T>(string json, JsonSerializerOptions options, out T? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<T>(json, options);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes an object to JSON with consistent options.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize</typeparam>
    /// <param name="value">The object to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the object</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string Serialize<T>(T value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? DefaultInternalOptionsIndented : DefaultInternalOptions;
        return JsonSerializer.Serialize(value, options);
    }
}