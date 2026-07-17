#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Middleware;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="NotificationPipeline"/> and related types
/// </summary>
public static class NotificationPipelineJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Converts a <see cref="NotificationPipeline"/> to a JSON string
    /// </summary>
    /// <param name="value">The notification pipeline to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON representation of the notification pipeline</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null</exception>
    public static string ToJson(this NotificationPipeline value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Parses a JSON string into a <see cref="NotificationPipeline"/> instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <returns>The deserialized <see cref="NotificationPipeline"/>, or null if parsing fails</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty</exception>
    public static NotificationPipeline? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<NotificationPipeline>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            // Log the exception for debugging while maintaining backward compatibility
            return null;
        }
    }

    /// <summary>
    /// Attempts to parse a JSON string into a <see cref="NotificationPipeline"/> instance
    /// </summary>
    /// <param name="json">The JSON string to parse</param>
    /// <param name="value">Receives the deserialized <see cref="NotificationPipeline"/> if successful</param>
    /// <returns>True if parsing succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty</exception>
    public static bool TryFromJson(string json, out NotificationPipeline? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<NotificationPipeline>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}