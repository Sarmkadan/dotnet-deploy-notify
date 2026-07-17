#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Utilities;

/// <summary>
/// System.Text.Json serialization extensions for enum values
/// </summary>
public static class EnumExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes the enum value to camelCase JSON string
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="value">The enum value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the enum value</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson<T>(this T value, bool indented = false) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes JSON string to enum value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized enum value</returns>
    /// <exception cref="ArgumentException">Thrown when JSON string is null or whitespace</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static T FromJson<T>(string json) where T : struct, Enum
    {
        ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

        var result = JsonSerializer.Deserialize<T?>(json, _jsonOptions);
        return result ?? throw new JsonException("Deserialization returned null for non-nullable enum type.");
    }

    /// <summary>
    /// Attempts to deserialize JSON string to enum value
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized value</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson<T>(string json, out T value) where T : struct, Enum
    {
        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            var result = JsonSerializer.Deserialize<T?>(json, _jsonOptions);
            if (result is not null)
            {
                value = result.Value;
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}