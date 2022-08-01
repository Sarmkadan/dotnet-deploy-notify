#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides JSON serialization/deserialization extensions for WebhookPayload
/// </summary>
public static class WebhookPayloadJsonExtensions
{
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the WebhookPayload to a JSON string
    /// </summary>
    /// <param name="value">The payload to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the payload</returns>
    public static string ToJson(this WebhookPayload value, bool indented = false)
    {
        if (value is null)
        {
            throw new System.ArgumentNullException(nameof(value));
        }

        var options = indented
            ? new System.Text.Json.JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return System.Text.Json.JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a WebhookPayload
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized WebhookPayload or null if parsing fails</returns>
    public static WebhookPayload? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<WebhookPayload>(json, _jsonOptions);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a WebhookPayload
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter containing the deserialized payload or null</param>
    /// <returns>True if deserialization succeeded, false otherwise</returns>
    public static bool TryFromJson(string json, out WebhookPayload? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = System.Text.Json.JsonSerializer.Deserialize<WebhookPayload>(json, _jsonOptions);
            return value is not null;
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }
    }
}