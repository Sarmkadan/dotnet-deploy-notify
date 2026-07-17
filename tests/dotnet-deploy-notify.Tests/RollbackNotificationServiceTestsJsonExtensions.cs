using System.Text.Json;

namespace DotNetDeployNotify.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for <see cref="RollbackNotificationServiceTests"/> objects.
/// </summary>
public static class RollbackNotificationServiceTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="RollbackNotificationServiceTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="RollbackNotificationServiceTests"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for better readability.</param>
    /// <returns>A JSON string representation of the <see cref="RollbackNotificationServiceTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when serialization fails.</exception>
    public static string ToJson(this RollbackNotificationServiceTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = GetJsonSerializerOptions(indented);
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="RollbackNotificationServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="RollbackNotificationServiceTests"/> instance if deserialization succeeds; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when deserialization fails.</exception>
    public static RollbackNotificationServiceTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<RollbackNotificationServiceTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="RollbackNotificationServiceTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="RollbackNotificationServiceTests"/> instance if successful; otherwise, null.</param>
    /// <returns>true if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when deserialization fails.</exception>
    public static bool TryFromJson(string json, out RollbackNotificationServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = JsonSerializer.Deserialize<RollbackNotificationServiceTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private static JsonSerializerOptions GetJsonSerializerOptions(bool indented)
    {
        var options = new JsonSerializerOptions(_jsonSerializerOptions)
        {
            WriteIndented = indented
        };
        return options;
    }
}