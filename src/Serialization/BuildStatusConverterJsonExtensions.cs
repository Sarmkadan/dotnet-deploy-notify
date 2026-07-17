#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System.Text.Json;
using DotNetDeployNotify.Core;

namespace DotNetDeployNotify.Serialization;

/// <summary>
/// Provides JSON serialization extension methods for <see cref="BuildStatusConverter"/>.
/// </summary>
public static class BuildStatusConverterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Converts a <see cref="BuildStatusConverter"/> instance to a JSON string representation.
    /// </summary>
    /// <param name="value">The converter instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the converter.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this BuildStatusConverter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="BuildStatus"/> value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="BuildStatus"/> value parsed from JSON.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to a valid <see cref="BuildStatus"/> value.</exception>
    public static BuildStatus FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        try
        {
            return JsonSerializer.Deserialize<BuildStatus>(json, _jsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to deserialize BuildStatus from JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="BuildStatus"/> value.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized BuildStatus value if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out BuildStatus value)
    {
        value = BuildStatus.Started; // Default value

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<BuildStatus>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}