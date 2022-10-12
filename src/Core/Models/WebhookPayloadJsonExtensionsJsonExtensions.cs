using System;
using System.Text.Json;

namespace DotNetDeployNotify.Core.Models;

public static class WebhookPayloadJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="WebhookPayload"/> instance to a JSON string using the WebhookPayloadJsonExtensions extensions.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">Whether to indent the JSON output for readability.</param>
    /// <returns>The JSON representation of the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this WebhookPayload value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return WebhookPayloadJsonExtensions.ToJson(value, indented);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="WebhookPayload"/> instance using the WebhookPayloadJsonExtensions extensions.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static WebhookPayload? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return WebhookPayloadJsonExtensions.FromJson(json);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="WebhookPayload"/> instance using the WebhookPayloadJsonExtensions extensions.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized instance, or <see langword="null"/> if deserialization fails.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out WebhookPayload? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        return WebhookPayloadJsonExtensions.TryFromJson(json, out value);
    }
}