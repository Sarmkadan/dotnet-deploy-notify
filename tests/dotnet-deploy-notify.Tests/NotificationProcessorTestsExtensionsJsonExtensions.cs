#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="NotificationProcessorTests"/>
/// to facilitate testing of notification processing scenarios.
/// </summary>
/// <remarks>
/// This class provides a shared <see cref="JsonSerializerOptions"/> instance configured with camelCase naming policy
/// and web defaults, suitable for serializing and deserializing test data created by
/// <see cref="NotificationProcessorTestsExtensions"/> extension methods.
/// </remarks>
public static class NotificationProcessorTestsExtensionsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options with camelCase naming policy, web defaults, and cycle reference handling.
    /// Suitable for serializing and deserializing types created by <see cref="NotificationProcessorTestsExtensions"/> extension methods.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes the specified <see cref="NotificationProcessorTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this NotificationProcessorTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="NotificationProcessorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or <see langword="null"/> if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static NotificationProcessorTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<NotificationProcessorTests>(json, JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="NotificationProcessorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out NotificationProcessorTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = default;

        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            value = JsonSerializer.Deserialize<NotificationProcessorTests>(json, JsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
