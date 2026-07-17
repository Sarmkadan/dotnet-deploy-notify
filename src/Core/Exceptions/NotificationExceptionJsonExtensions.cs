#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetDeployNotify.Core.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="NotificationException"/>.
/// </summary>
/// <remarks>
/// This class is static and cannot be inherited.
/// </remarks>
public static class NotificationExceptionJsonExtensions
{
    /// <summary>
    /// Shared JSON serialization options with camelCase naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="NotificationException"/> instance to JSON.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this NotificationException value, bool indented = false)
        => JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true } : JsonOptions);

    /// <summary>
    /// Deserializes a JSON string to a <see cref="NotificationException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="NotificationException"/> instance, or <see langword="null"/> if deserialization failed.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
    public static NotificationException? FromJson(string json)
        => JsonSerializer.Deserialize<NotificationException>(json, JsonOptions);

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="NotificationException"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/>, empty, or consists only of white-space characters.</exception>
    public static bool TryFromJson(string json, out NotificationException? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<NotificationException>(json, JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
