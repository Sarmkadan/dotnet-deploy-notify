#nullable enable

namespace DotNetDeployNotify.Core.Models;

/// <summary>
/// Provides JSON serialization/deserialization extensions for <see cref="WebhookPayload"/>.
/// </summary>
public static class WebhookPayloadJsonExtensions
{
    private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the WebhookPayload to a JSON string.
    /// </summary>
    /// <param name="value">The payload to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the payload.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this WebhookPayload value, bool indented = false)
        => value is null
            ? throw new System.ArgumentNullException(nameof(value))
            : System.Text.Json.JsonSerializer.Serialize(
                value,
                indented
                    ? new System.Text.Json.JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                    : _jsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a WebhookPayload.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized WebhookPayload or <see langword="null"/> if parsing fails.</returns>
    public static WebhookPayload? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

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
    /// Attempts to deserialize a JSON string to a WebhookPayload.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter containing the deserialized payload or <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out WebhookPayload? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = System.Text.Json.JsonSerializer.Deserialize<WebhookPayload>(json, _jsonOptions);
            return value is not null;
        }
        catch (System.Text.Json.JsonException)
        {
            value = null;
            return false;
        }
    }
}